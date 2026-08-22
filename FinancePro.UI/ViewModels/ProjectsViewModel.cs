using System.Collections.ObjectModel;
using System.Windows.Input;
using FinancePro.Platform.Accounting;
using FinancePro.UI.Common;

namespace FinancePro.UI.ViewModels;

public sealed class ProjectsViewModel : ViewModelBase
{
    private readonly IAnalyticAccountingService _service; private readonly int _companyId;
    private string _code="",_name="",_status="Planeado",_message="",_workNumber="",_workDescription="",_workStatus="Aberta"; private int? _projectId,_workOrderId;
    private DateTime _start=DateTime.Today,_workDate=DateTime.Today; private DateTime? _end,_workDue;
    private decimal _budgetRevenue,_budgetCost,_workEstimated; private ProjectClientOption? _client; private AnalyticCostCenter? _costCenter; private ProjectRow? _selectedProject;
    public ObservableCollection<ProjectRow> Projects{get;}=new(); public ObservableCollection<WorkOrderRow> WorkOrders{get;}=new(); public ObservableCollection<ProjectClientOption> Clients{get;}=new(); public ObservableCollection<AnalyticCostCenter> CostCenters{get;}=new();
    public IReadOnlyList<string> ProjectStatuses{get;}=["Planeado","Em execução","Suspenso","Concluído","Cancelado"]; public IReadOnlyList<string> WorkStatuses{get;}=["Aberta","Em execução","Concluída","Cancelada"];
    public string Code{get=>_code;set=>SetProperty(ref _code,value);} public string Name{get=>_name;set=>SetProperty(ref _name,value);} public string Status{get=>_status;set=>SetProperty(ref _status,value);} public string Message{get=>_message;set=>SetProperty(ref _message,value);}
    public DateTime Start{get=>_start;set=>SetProperty(ref _start,value);} public DateTime? End{get=>_end;set=>SetProperty(ref _end,value);} public decimal BudgetRevenue{get=>_budgetRevenue;set=>SetProperty(ref _budgetRevenue,value);} public decimal BudgetCost{get=>_budgetCost;set=>SetProperty(ref _budgetCost,value);}
    public ProjectClientOption? Client{get=>_client;set=>SetProperty(ref _client,value);} public AnalyticCostCenter? CostCenter{get=>_costCenter;set=>SetProperty(ref _costCenter,value);}
    public ProjectRow? SelectedProject{get=>_selectedProject;set{if(SetProperty(ref _selectedProject,value))_=LoadWorkOrdersAsync();}}
    public string WorkNumber{get=>_workNumber;set=>SetProperty(ref _workNumber,value);} public string WorkDescription{get=>_workDescription;set=>SetProperty(ref _workDescription,value);} public string WorkStatus{get=>_workStatus;set=>SetProperty(ref _workStatus,value);} public DateTime WorkDate{get=>_workDate;set=>SetProperty(ref _workDate,value);} public DateTime? WorkDue{get=>_workDue;set=>SetProperty(ref _workDue,value);} public decimal WorkEstimated{get=>_workEstimated;set=>SetProperty(ref _workEstimated,value);}
    public decimal TotalRevenue=>Projects.Sum(x=>x.Revenue); public decimal TotalCost=>Projects.Sum(x=>x.Cost); public decimal TotalResult=>Projects.Sum(x=>x.Result); public int ActiveProjects=>Projects.Count(x=>x.Status is not "Concluído" and not "Cancelado");
    public ICommand RefreshCommand{get;} public ICommand SaveProjectCommand{get;} public ICommand NewProjectCommand{get;} public ICommand EditProjectCommand{get;} public ICommand SaveWorkOrderCommand{get;} public ICommand EditWorkOrderCommand{get;}
    public ProjectsViewModel(IAnalyticAccountingService service,int companyId){_service=service;_companyId=companyId;RefreshCommand=new AsyncRelayCommand(_=>LoadAsync());SaveProjectCommand=new AsyncRelayCommand(_=>SaveProjectAsync());NewProjectCommand=new AsyncRelayCommand(_=>{ClearProject();return Task.CompletedTask;});EditProjectCommand=new RelayCommand(EditProject);SaveWorkOrderCommand=new AsyncRelayCommand(_=>SaveWorkOrderAsync());EditWorkOrderCommand=new RelayCommand(EditWorkOrder);_=LoadAsync();}
    private async Task LoadAsync(){try{Replace(Projects,await _service.ListProjectsAsync(_companyId));Replace(Clients,await _service.ListProjectClientsAsync(_companyId));Replace(CostCenters,await _service.ListCostCentersAsync(_companyId));NotifyTotals();await LoadWorkOrdersAsync();}catch(Exception ex){Message=ex.Message;}}
    private async Task LoadWorkOrdersAsync(){try{Replace(WorkOrders,await _service.ListWorkOrdersAsync(_companyId,SelectedProject?.Id));}catch(Exception ex){Message=ex.Message;}}
    private async Task SaveProjectAsync(){try{await _service.SaveProjectAsync(new(_companyId,_projectId,Code,Name,Client?.Id,CostCenter?.Id,Start,End,BudgetRevenue,BudgetCost,Status));Message="Projeto guardado.";ClearProject();await LoadAsync();}catch(Exception ex){Message=ex.Message;}}
    private async Task SaveWorkOrderAsync(){if(SelectedProject is null){Message="Selecione um projeto para criar a ordem de trabalho.";return;}try{await _service.SaveWorkOrderAsync(new(_companyId,_workOrderId,SelectedProject.Id,WorkNumber,WorkDescription,WorkDate,WorkDue,WorkEstimated,WorkStatus));_workOrderId=null;WorkNumber=WorkDescription="";WorkEstimated=0;WorkDue=null;Message="Ordem de trabalho guardada.";await LoadWorkOrdersAsync();}catch(Exception ex){Message=ex.Message;}}
    private void EditProject(object? p){if(p is ProjectRow x){_projectId=x.Id;SelectedProject=x;Code=x.Code;Name=x.Name;Client=Clients.FirstOrDefault(c=>c.Name==x.Client);CostCenter=CostCenters.FirstOrDefault(c=>c.Name==x.CostCenter);Start=x.StartDate;End=x.EndDate;BudgetRevenue=x.BudgetRevenue;BudgetCost=x.BudgetCost;Status=x.Status;Message="Projeto carregado para edição.";}}
    private void EditWorkOrder(object? p){if(p is WorkOrderRow x){_workOrderId=x.Id;SelectedProject=Projects.FirstOrDefault(y=>y.Id==x.ProjectId)??SelectedProject;WorkNumber=x.Number;WorkDescription=x.Description;WorkDate=x.OrderDate;WorkDue=x.DueDate;WorkEstimated=x.EstimatedCost;WorkStatus=x.Status;Message="Ordem carregada para edição.";}}
    private void ClearProject(){_projectId=null;Code=Name="";Status="Planeado";Start=DateTime.Today;End=null;BudgetRevenue=BudgetCost=0;Client=null;CostCenter=null;}
    private void NotifyTotals(){foreach(var n in new[]{nameof(TotalRevenue),nameof(TotalCost),nameof(TotalResult),nameof(ActiveProjects)})OnPropertyChanged(n);}
    private static void Replace<T>(ObservableCollection<T> t,IEnumerable<T> s){t.Clear();foreach(var x in s)t.Add(x);}
}

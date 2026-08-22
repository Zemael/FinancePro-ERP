using System.Collections.ObjectModel;
using System.Windows.Input;
using FinancePro.Platform.Accounting;
using FinancePro.UI.Common;

namespace FinancePro.UI.ViewModels;
public sealed class AnalyticAccountingViewModel:ViewModelBase
{
 private readonly IAnalyticAccountingService _service;private readonly int _companyId;private int? _departmentId;private bool _departmentActive=true;private string _code="",_name="",_message="";private DateTime _from=new(DateTime.Today.Year,1,1),_to=DateTime.Today;private Department? _department;private AnalyticCostCenter? _costCenter;private AnalyticAccountingSummary _summary=new(0,0,0,0,0,0,0);
 public ObservableCollection<Department> Departments{get;}=new();public ObservableCollection<AnalyticCostCenter> CostCenters{get;}=new();public ObservableCollection<CostCenterPerformanceRow> Performance{get;}=new();
 public string Code{get=>_code;set=>SetProperty(ref _code,value);}public string Name{get=>_name;set=>SetProperty(ref _name,value);}public string Message{get=>_message;set=>SetProperty(ref _message,value);}public DateTime From{get=>_from;set=>SetProperty(ref _from,value);}public DateTime To{get=>_to;set=>SetProperty(ref _to,value);}public Department? SelectedDepartment{get=>_department;set=>SetProperty(ref _department,value);}public AnalyticCostCenter? SelectedCostCenter{get=>_costCenter;set=>SetProperty(ref _costCenter,value);}
 public decimal Budgeted=>_summary.Budgeted;public decimal Committed=>_summary.Committed;public decimal Realized=>_summary.Realized;public decimal Revenue=>_summary.Revenue;public decimal Expense=>_summary.Expense;public decimal Result=>_summary.Result;public decimal Variance=>_summary.Variance;
 public ICommand RefreshCommand{get;}public ICommand SaveDepartmentCommand{get;}public ICommand EditDepartmentCommand{get;}public ICommand ToggleDepartmentCommand{get;}public ICommand AssignDepartmentCommand{get;}public ICommand GenerateCommand{get;}
 public AnalyticAccountingViewModel(IAnalyticAccountingService service,int companyId){_service=service;_companyId=companyId;RefreshCommand=new AsyncRelayCommand(_=>LoadAsync());SaveDepartmentCommand=new AsyncRelayCommand(_=>SaveAsync());EditDepartmentCommand=new RelayCommand(EditDepartment);ToggleDepartmentCommand=new AsyncRelayCommand(ToggleAsync);AssignDepartmentCommand=new AsyncRelayCommand(_=>AssignAsync());GenerateCommand=new AsyncRelayCommand(_=>GenerateAsync());_=LoadAsync();}
 private async Task LoadAsync(){try{Replace(Departments,await _service.ListDepartmentsAsync(_companyId));Replace(CostCenters,await _service.ListCostCentersAsync(_companyId));await GenerateAsync();}catch(Exception ex){Message=ex.Message;}}
 private async Task SaveAsync(){try{await _service.SaveDepartmentAsync(new(_companyId,_departmentId,Code,Name,_departmentActive));_departmentId=null;_departmentActive=true;Code=Name="";Message="Departamento guardado.";await LoadAsync();}catch(Exception ex){Message=ex.Message;}}
 private void EditDepartment(object? p){if(p is Department d){_departmentId=d.Id;_departmentActive=d.Active;Code=d.Code;Name=d.Name;Message="Departamento carregado para edição.";}}
 private async Task ToggleAsync(object? p){if(p is Department d){await _service.SetDepartmentActiveAsync(_companyId,d.Id,!d.Active);await LoadAsync();}}
 private async Task AssignAsync(){if(SelectedCostCenter is null)return;await _service.AssignCostCenterDepartmentAsync(_companyId,SelectedCostCenter.Id,SelectedDepartment?.Id);Message="Centro de custo associado.";await LoadAsync();}
 private async Task GenerateAsync(){try{Replace(Performance,await _service.GetPerformanceAsync(_companyId,From,To));_summary=await _service.GetSummaryAsync(_companyId,From,To);foreach(var n in new[]{nameof(Budgeted),nameof(Committed),nameof(Realized),nameof(Revenue),nameof(Expense),nameof(Result),nameof(Variance)})OnPropertyChanged(n);}catch(Exception ex){Message=ex.Message;}}
 private static void Replace<T>(ObservableCollection<T> t,IEnumerable<T> s){t.Clear();foreach(var x in s)t.Add(x);}
}

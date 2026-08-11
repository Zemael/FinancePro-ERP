using System.Collections.ObjectModel;
using System.Windows.Input;
using FinancePro.Platform.Consolidation;
using FinancePro.UI.Common;

namespace FinancePro.UI.ViewModels;

public sealed class ConsolidationCompanyOption : ViewModelBase
{
    public ConsolidationCompanyOption(ConsolidationCompany company) => Company = company;
    public ConsolidationCompany Company { get; }
    private bool _isSelected;
    public bool IsSelected { get => _isSelected; set => SetProperty(ref _isSelected, value); }
    public int Id => Company.Id;
    public string Name => Company.Name;
}

public sealed class ConsolidationViewModel : ViewModelBase
{
    private readonly IConsolidationService _service; private readonly int _userId; private readonly string _userName;
    private ConsolidationGroup? _selectedGroup; private ConsolidationRun? _selectedRun; private ConsolidationElimination? _selectedElimination; private ConsolidatedTrialBalanceRow? _selectedTrialRow; private IntercompanyDifference? _selectedDifference;
    private string _groupName="Grupo FinancePro"; private int _fiscalYear=DateTime.Today.Year; private DateTime _fromDate=new(DateTime.Today.Year,1,1); private DateTime _toDate=DateTime.Today;
    private string _eliminationAccount=string.Empty; private string _eliminationDescription=string.Empty; private string _eliminationReference=string.Empty; private decimal _eliminationDebit; private decimal _eliminationCredit; private string _message=string.Empty; private bool _busy;
    private ConsolidationSummary _summary=new(0,0,0,0,0,0,0,0,0); private ConsolidationValidationResult? _validation;

    public ObservableCollection<ConsolidationCompanyOption> Companies { get; }=new();
    public ObservableCollection<ConsolidationGroup> Groups { get; }=new();
    public ObservableCollection<ConsolidationRun> Runs { get; }=new();
    public ObservableCollection<ConsolidatedTrialBalanceRow> TrialBalanceRows { get; }=new();
    public ObservableCollection<IntercompanyDifference> Differences { get; }=new();
    public ObservableCollection<ConsolidationElimination> Eliminations { get; }=new();
    public ObservableCollection<ConsolidationValidationCheck> ValidationChecks { get; }=new();

    public ConsolidationGroup? SelectedGroup { get=>_selectedGroup; set { if(SetProperty(ref _selectedGroup,value)){ApplyGroupSelection();_=LoadRunsAsync();} } }
    public ConsolidationRun? SelectedRun { get=>_selectedRun; set { if(SetProperty(ref _selectedRun,value)) _=LoadRunDetailsAsync(); } }
    public ConsolidationElimination? SelectedElimination { get=>_selectedElimination; set=>SetProperty(ref _selectedElimination,value); }
    public ConsolidatedTrialBalanceRow? SelectedTrialRow { get=>_selectedTrialRow; set=>SetProperty(ref _selectedTrialRow,value); }
    public IntercompanyDifference? SelectedDifference { get=>_selectedDifference; set=>SetProperty(ref _selectedDifference,value); }
    public string GroupName { get=>_groupName; set=>SetProperty(ref _groupName,value); }
    public int FiscalYear { get=>_fiscalYear; set=>SetProperty(ref _fiscalYear,value); }
    public DateTime FromDate { get=>_fromDate; set=>SetProperty(ref _fromDate,value); }
    public DateTime ToDate { get=>_toDate; set=>SetProperty(ref _toDate,value); }
    public string EliminationAccount { get=>_eliminationAccount; set=>SetProperty(ref _eliminationAccount,value); }
    public string EliminationDescription { get=>_eliminationDescription; set=>SetProperty(ref _eliminationDescription,value); }
    public string EliminationReference { get=>_eliminationReference; set=>SetProperty(ref _eliminationReference,value); }
    public decimal EliminationDebit { get=>_eliminationDebit; set=>SetProperty(ref _eliminationDebit,value); }
    public decimal EliminationCredit { get=>_eliminationCredit; set=>SetProperty(ref _eliminationCredit,value); }
    public string Message { get=>_message; set=>SetProperty(ref _message,value); }
    public bool Busy { get=>_busy; set=>SetProperty(ref _busy,value); }
    public string CurrentStatus => SelectedRun?.Status ?? "Sem processamento";
    public int CompanyCount=>_summary.CompanyCount; public int AccountCount=>_summary.AccountCount; public decimal ConsolidatedDebit=>_summary.ConsolidatedDebit; public decimal ConsolidatedCredit=>_summary.ConsolidatedCredit; public int DifferenceCount=>_summary.IntercompanyDifferences; public bool IsBalanced=>_summary.IsBalanced;

    public ICommand RefreshCommand { get; } public ICommand SaveGroupCommand { get; } public ICommand CreateRunCommand { get; } public ICommand ValidateCommand { get; } public ICommand ConsolidateCommand { get; } public ICommand CloseCommand { get; }
    public ICommand AddEliminationCommand { get; } public ICommand DeleteEliminationCommand { get; } public ICommand PrepareEliminationCommand { get; }

    public ConsolidationViewModel(IConsolidationService service,int userId,string userName)
    {
        _service=service;_userId=userId;_userName=userName;
        RefreshCommand=new AsyncRelayCommand(_=>LoadAsync()); SaveGroupCommand=new AsyncRelayCommand(_=>SaveGroupAsync()); CreateRunCommand=new AsyncRelayCommand(_=>CreateRunAsync());
        ValidateCommand=new AsyncRelayCommand(_=>ValidateAsync(),_=>SelectedRun is not null && SelectedRun.Status!=ConsolidationRunStatus.Closed);
        ConsolidateCommand=new AsyncRelayCommand(_=>ConsolidateAsync(),_=>SelectedRun?.Status==ConsolidationRunStatus.Validated);
        CloseCommand=new AsyncRelayCommand(_=>CloseAsync(),_=>SelectedRun?.Status==ConsolidationRunStatus.Consolidated);
        AddEliminationCommand=new AsyncRelayCommand(_=>AddEliminationAsync(),_=>SelectedRun is not null); DeleteEliminationCommand=new AsyncRelayCommand(_=>DeleteEliminationAsync(),_=>SelectedElimination is not null);
        PrepareEliminationCommand=new RelayCommand(_=>PrepareElimination(),_=>SelectedDifference is not null && SelectedTrialRow is not null);
        _=LoadAsync();
    }

    private async Task LoadAsync(){Busy=true;Message=string.Empty;try{var companies=await _service.ListCompaniesAsync();var groups=await _service.ListGroupsAsync();Companies.Clear();foreach(var x in companies)Companies.Add(new(x));Groups.Clear();foreach(var x in groups)Groups.Add(x);SelectedGroup=Groups.FirstOrDefault();}catch(Exception ex){Message=ex.Message;}finally{Busy=false;}}
    private void ApplyGroupSelection(){if(SelectedGroup is null)return;GroupName=SelectedGroup.Name;FiscalYear=SelectedGroup.FiscalYear;foreach(var x in Companies)x.IsSelected=SelectedGroup.CompanyIds.Contains(x.Id);}
    private async Task SaveGroupAsync(){try{var ids=Companies.Where(x=>x.IsSelected).Select(x=>x.Id).ToList();var id=await _service.SaveGroupAsync(new(SelectedGroup?.Id,GroupName,FiscalYear,ids));Message="Grupo de consolidação guardado.";await ReloadGroupsAsync(id);}catch(Exception ex){Message=ex.Message;}}
    private async Task ReloadGroupsAsync(int? selectId=null){var groups=await _service.ListGroupsAsync();Groups.Clear();foreach(var x in groups)Groups.Add(x);SelectedGroup=Groups.FirstOrDefault(x=>x.Id==selectId)??Groups.FirstOrDefault();}
    private async Task LoadRunsAsync(){Runs.Clear();TrialBalanceRows.Clear();Differences.Clear();Eliminations.Clear();ValidationChecks.Clear();if(SelectedGroup is null)return;var runs=await _service.ListRunsAsync(SelectedGroup.Id);foreach(var x in runs)Runs.Add(x);SelectedRun=Runs.FirstOrDefault();}
    private async Task CreateRunAsync(){try{if(SelectedGroup is null)throw new InvalidOperationException("Guarde ou selecione um grupo primeiro.");var id=await _service.CreateRunAsync(new(SelectedGroup.Id,FromDate,ToDate,_userId,_userName));Message="Processamento criado em rascunho.";await LoadRunsAsync();SelectedRun=Runs.FirstOrDefault(x=>x.Id==id)??Runs.FirstOrDefault();}catch(Exception ex){Message=ex.Message;}}
    private async Task LoadRunDetailsAsync(){OnPropertyChanged(nameof(CurrentStatus));ValidationChecks.Clear();TrialBalanceRows.Clear();Differences.Clear();Eliminations.Clear();if(SelectedRun is null){_summary=new(0,0,0,0,0,0,0,0,0);NotifySummary();return;}try{var tb=await _service.GetTrialBalanceAsync(SelectedRun.Id);var diffs=await _service.GetIntercompanyDifferencesAsync(SelectedRun.Id);var elims=await _service.ListEliminationsAsync(SelectedRun.Id);_summary=await _service.GetSummaryAsync(SelectedRun.Id);foreach(var x in tb)TrialBalanceRows.Add(x);foreach(var x in diffs)Differences.Add(x);foreach(var x in elims)Eliminations.Add(x);NotifySummary();}catch(Exception ex){Message=ex.Message;}}
    private void NotifySummary(){OnPropertyChanged(nameof(CompanyCount));OnPropertyChanged(nameof(AccountCount));OnPropertyChanged(nameof(ConsolidatedDebit));OnPropertyChanged(nameof(ConsolidatedCredit));OnPropertyChanged(nameof(DifferenceCount));OnPropertyChanged(nameof(IsBalanced));OnPropertyChanged(nameof(CurrentStatus));}
    private async Task ValidateAsync(){if(SelectedRun is null)return;try{_validation=await _service.ValidateAsync(SelectedRun.Id,_userId,_userName);ValidationChecks.Clear();foreach(var x in _validation.Checks)ValidationChecks.Add(x);Message=_validation.CanValidate?"Consolidação validada com sucesso.":$"Validação bloqueada: {_validation.BlockingIssues} pendência(s).";await RefreshSelectedRunAsync();}catch(Exception ex){Message=ex.Message;}}
    private async Task ConsolidateAsync(){if(SelectedRun is null)return;try{await _service.ConsolidateAsync(SelectedRun.Id,_userId,_userName);Message="Consolidação processada com sucesso.";await RefreshSelectedRunAsync();}catch(Exception ex){Message=ex.Message;}}
    private async Task CloseAsync(){if(SelectedRun is null)return;try{await _service.CloseAsync(SelectedRun.Id,_userId,_userName);Message="Consolidação fechada.";await RefreshSelectedRunAsync();}catch(Exception ex){Message=ex.Message;}}
    private async Task RefreshSelectedRunAsync(){var id=SelectedRun?.Id;await LoadRunsAsync();SelectedRun=Runs.FirstOrDefault(x=>x.Id==id)??Runs.FirstOrDefault();}
    private void PrepareElimination(){if(SelectedDifference is null||SelectedTrialRow is null)return;EliminationAccount=SelectedTrialRow.AccountCode;EliminationReference=$"ELIM-{SelectedDifference.Reference}";EliminationDescription=$"Eliminação intercompany — {SelectedDifference.Reference}";EliminationDebit=0;EliminationCredit=SelectedDifference.Difference;Message="Sugestão preparada. Confirme débito/crédito e contrapartida antes de validar.";}
    private async Task AddEliminationAsync(){if(SelectedRun is null)return;try{await _service.SaveEliminationAsync(new(SelectedRun.Id,EliminationAccount,EliminationDescription,EliminationDebit,EliminationCredit,EliminationReference,_userId,_userName));EliminationAccount=string.Empty;EliminationDescription=string.Empty;EliminationReference=string.Empty;EliminationDebit=0;EliminationCredit=0;Message="Eliminação adicionada.";await LoadRunDetailsAsync();}catch(Exception ex){Message=ex.Message;}}
    private async Task DeleteEliminationAsync(){if(SelectedRun is null||SelectedElimination is null)return;try{await _service.DeleteEliminationAsync(SelectedRun.Id,SelectedElimination.Id);Message="Eliminação removida.";await LoadRunDetailsAsync();}catch(Exception ex){Message=ex.Message;}}
}

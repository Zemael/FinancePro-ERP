using System.Collections.ObjectModel;
using System.Windows.Input;
using FinancePro.Platform.Accounting;
using FinancePro.Platform.Administration;
using FinancePro.UI.Common;

namespace FinancePro.UI.ViewModels;

public sealed class AccountingDraftLineViewModel : ViewModelBase
{
    private ChartAccount? _account; private CostCenter? _costCenter; private string _description=string.Empty; private decimal _debit; private decimal _credit;
    public ChartAccount? Account { get=>_account; set=>SetProperty(ref _account,value); }
    public CostCenter? CostCenter { get=>_costCenter; set=>SetProperty(ref _costCenter,value); }
    public string Description { get=>_description; set=>SetProperty(ref _description,value); }
    public decimal Debit { get=>_debit; set=>SetProperty(ref _debit,value); }
    public decimal Credit { get=>_credit; set=>SetProperty(ref _credit,value); }
}

public sealed class AccountingViewModel : ViewModelBase
{
    private readonly IAccountingService _service; private readonly IAdministrationMasterDataService _masterData; private readonly int _companyId; private readonly int _userId; private readonly string _userName;
    private AccountingEntry? _selectedEntry; private ChartAccount? _selectedLedgerAccount; private DateTime _entryDate=DateTime.Today; private DateTime _reportFrom=new(DateTime.Today.Year,DateTime.Today.Month,1); private DateTime _reportTo=DateTime.Today; private DateTime _previousReportFrom=new DateTime(DateTime.Today.Year,DateTime.Today.Month,1).AddMonths(-1); private DateTime _previousReportTo=new DateTime(DateTime.Today.Year,DateTime.Today.Month,1).AddDays(-1);
    private IncomeStatementSummary _incomeSummary=new(0,0,0,0,0,0,0,0,0,0,0,0); private BalanceSheetSummary _balanceSummary=new(0,0,0,0,0,0,0); private string _documentNumber=$"CTB-{DateTime.Today:yyyyMMdd-HHmm}"; private string _reference=string.Empty; private string _description=string.Empty; private string _message=string.Empty; private string _journalSearch=string.Empty; private string _journalStatus=string.Empty; private bool _busy; private AccountingEntrySummary _summary=new(0,0,0,0); private TrialBalanceSummary _trialSummary=new(0,0,0,0,0,0);

    public ObservableCollection<AccountingEntry> Entries { get; }=new(); public ObservableCollection<ChartAccount> Accounts { get; }=new(); public ObservableCollection<CostCenter> CostCenters { get; }=new(); public ObservableCollection<AccountingDraftLineViewModel> DraftLines { get; }=new();
    public ObservableCollection<GeneralJournalRow> JournalRows { get; }=new(); public ObservableCollection<GeneralLedgerRow> LedgerRows { get; }=new(); public ObservableCollection<TrialBalanceRow> TrialBalanceRows { get; }=new(); public ObservableCollection<IncomeStatementRow> IncomeStatementRows { get; }=new(); public ObservableCollection<BalanceSheetRow> BalanceSheetRows { get; }=new();
    public IReadOnlyList<string> JournalStatuses { get; }=[string.Empty,AccountingEntryStatus.Draft,AccountingEntryStatus.Posted,AccountingEntryStatus.Reversed];
    public AccountingEntry? SelectedEntry { get=>_selectedEntry; set=>SetProperty(ref _selectedEntry,value); }
    public ChartAccount? SelectedLedgerAccount { get=>_selectedLedgerAccount; set=>SetProperty(ref _selectedLedgerAccount,value); }
    public DateTime EntryDate { get=>_entryDate; set=>SetProperty(ref _entryDate,value); }
    public DateTime ReportFrom { get=>_reportFrom; set=>SetProperty(ref _reportFrom,value); }
    public DateTime ReportTo { get=>_reportTo; set=>SetProperty(ref _reportTo,value); }
    public DateTime PreviousReportFrom { get=>_previousReportFrom; set=>SetProperty(ref _previousReportFrom,value); }
    public DateTime PreviousReportTo { get=>_previousReportTo; set=>SetProperty(ref _previousReportTo,value); }
    public string DocumentNumber { get=>_documentNumber; set=>SetProperty(ref _documentNumber,value); }
    public string Reference { get=>_reference; set=>SetProperty(ref _reference,value); }
    public string Description { get=>_description; set=>SetProperty(ref _description,value); }
    public string JournalSearch { get=>_journalSearch; set=>SetProperty(ref _journalSearch,value); }
    public string JournalStatus { get=>_journalStatus; set=>SetProperty(ref _journalStatus,value); }
    public string Message { get=>_message; set=>SetProperty(ref _message,value); }
    public bool Busy { get=>_busy; set=>SetProperty(ref _busy,value); }
    public int Drafts=>_summary.Drafts; public int Posted=>_summary.Posted; public decimal TotalDebit=>_summary.TotalDebit; public decimal TotalCredit=>_summary.TotalCredit;
    public decimal GrossRevenue=>_incomeSummary.GrossRevenue; public decimal NetRevenue=>_incomeSummary.NetRevenue; public decimal GrossProfit=>_incomeSummary.GrossProfit; public decimal OperatingResult=>_incomeSummary.OperatingResult; public decimal NetResult=>_incomeSummary.NetResult; public decimal PreviousNetResult=>_incomeSummary.PreviousNetResult; public decimal OperatingMargin=>_incomeSummary.OperatingMargin; public decimal NetMargin=>_incomeSummary.NetMargin;
    public decimal BalanceAssets=>_balanceSummary.TotalAssets; public decimal BalanceLiabilities=>_balanceSummary.TotalLiabilities; public decimal BalanceEquity=>_balanceSummary.Equity; public decimal BalanceDifference=>_balanceSummary.Difference; public bool BalanceIsBalanced=>_balanceSummary.IsBalanced;
    public decimal TrialPeriodDebit=>_trialSummary.PeriodDebit; public decimal TrialPeriodCredit=>_trialSummary.PeriodCredit; public decimal TrialClosingDebit=>_trialSummary.ClosingDebit; public decimal TrialClosingCredit=>_trialSummary.ClosingCredit; public bool TrialIsBalanced=>_trialSummary.IsBalanced;

    public ICommand RefreshCommand { get; } public ICommand AddLineCommand { get; } public ICommand RemoveLineCommand { get; } public ICommand SaveCommand { get; } public ICommand PostCommand { get; } public ICommand ReverseCommand { get; } public ICommand NewCommand { get; }
    public ICommand LoadJournalCommand { get; } public ICommand LoadLedgerCommand { get; } public ICommand LoadTrialBalanceCommand { get; } public ICommand LoadIncomeStatementCommand { get; } public ICommand LoadBalanceSheetCommand { get; } public ICommand ExportJournalCommand { get; } public ICommand ExportLedgerCommand { get; } public ICommand ExportTrialBalanceCommand { get; } public ICommand ExportIncomeStatementCommand { get; } public ICommand ExportBalanceSheetCommand { get; }

    public AccountingViewModel(IAccountingService service,IAdministrationMasterDataService masterData,int companyId,int userId,string userName)
    {
        _service=service;_masterData=masterData;_companyId=companyId;_userId=userId;_userName=userName;
        RefreshCommand=new AsyncRelayCommand(_=>LoadAsync()); AddLineCommand=new RelayCommand(_=>DraftLines.Add(new())); RemoveLineCommand=new RelayCommand(x=>{if(x is AccountingDraftLineViewModel l)DraftLines.Remove(l);},x=>x is AccountingDraftLineViewModel&&DraftLines.Count>2);
        SaveCommand=new AsyncRelayCommand(_=>SaveAsync()); PostCommand=new AsyncRelayCommand(_=>PostAsync(),_=>SelectedEntry?.Status==AccountingEntryStatus.Draft); ReverseCommand=new AsyncRelayCommand(_=>ReverseAsync(),_=>SelectedEntry?.Status==AccountingEntryStatus.Posted); NewCommand=new RelayCommand(_=>ResetDraft());
        LoadJournalCommand=new AsyncRelayCommand(_=>LoadJournalAsync()); LoadLedgerCommand=new AsyncRelayCommand(_=>LoadLedgerAsync()); LoadTrialBalanceCommand=new AsyncRelayCommand(_=>LoadTrialBalanceAsync()); LoadIncomeStatementCommand=new AsyncRelayCommand(_=>LoadIncomeStatementAsync()); LoadBalanceSheetCommand=new AsyncRelayCommand(_=>LoadBalanceSheetAsync());
        ExportJournalCommand=new RelayCommand(_=>ExportJournal(),_=>JournalRows.Count>0); ExportLedgerCommand=new RelayCommand(_=>ExportLedger(),_=>LedgerRows.Count>0); ExportTrialBalanceCommand=new RelayCommand(_=>ExportTrialBalance(),_=>TrialBalanceRows.Count>0); ExportIncomeStatementCommand=new RelayCommand(_=>ExportIncomeStatement(),_=>IncomeStatementRows.Count>0); ExportBalanceSheetCommand=new RelayCommand(_=>ExportBalanceSheet(),_=>BalanceSheetRows.Count>0);
        ResetDraft();_=LoadAsync();
    }

    private async Task LoadAsync(){Busy=true;Message=string.Empty;try{var entries=await _service.ListAsync(_companyId);var accounts=(await _masterData.ListChartAccountsAsync(_companyId)).Where(x=>x.Active&&x.AllowsPosting).ToList();var centers=(await _masterData.ListCostCentersAsync(_companyId)).Where(x=>x.Active).ToList();_summary=await _service.GetSummaryAsync(_companyId);Entries.Clear();foreach(var x in entries)Entries.Add(x);Accounts.Clear();foreach(var x in accounts)Accounts.Add(x);CostCenters.Clear();foreach(var x in centers)CostCenters.Add(x);OnPropertyChanged(nameof(Drafts));OnPropertyChanged(nameof(Posted));OnPropertyChanged(nameof(TotalDebit));OnPropertyChanged(nameof(TotalCredit));SelectedEntry=Entries.FirstOrDefault();SelectedLedgerAccount??=Accounts.FirstOrDefault();await LoadJournalAsync();await LoadTrialBalanceAsync();await LoadIncomeStatementAsync();await LoadBalanceSheetAsync();}catch(Exception ex){Message=ex.Message;}finally{Busy=false;}}
    private async Task LoadJournalAsync(){try{var rows=await _service.GetGeneralJournalAsync(_companyId,ReportFrom,ReportTo,JournalStatus,JournalSearch);JournalRows.Clear();foreach(var x in rows)JournalRows.Add(x);}catch(Exception ex){Message=ex.Message;}}
    private async Task LoadLedgerAsync(){try{if(SelectedLedgerAccount is null)throw new InvalidOperationException("Selecione uma conta contabilística.");var rows=await _service.GetGeneralLedgerAsync(_companyId,SelectedLedgerAccount.Id,ReportFrom,ReportTo);LedgerRows.Clear();foreach(var x in rows)LedgerRows.Add(x);}catch(Exception ex){Message=ex.Message;}}
    private async Task LoadTrialBalanceAsync(){try{var rows=await _service.GetTrialBalanceAsync(_companyId,ReportFrom,ReportTo);_trialSummary=await _service.GetTrialBalanceSummaryAsync(_companyId,ReportFrom,ReportTo);TrialBalanceRows.Clear();foreach(var x in rows)TrialBalanceRows.Add(x);OnPropertyChanged(nameof(TrialPeriodDebit));OnPropertyChanged(nameof(TrialPeriodCredit));OnPropertyChanged(nameof(TrialClosingDebit));OnPropertyChanged(nameof(TrialClosingCredit));OnPropertyChanged(nameof(TrialIsBalanced));}catch(Exception ex){Message=ex.Message;}}

    private async Task LoadBalanceSheetAsync(){try{var rows=await _service.GetBalanceSheetAsync(_companyId,ReportTo,PreviousReportTo);_balanceSummary=await _service.GetBalanceSheetSummaryAsync(_companyId,ReportTo,PreviousReportTo);BalanceSheetRows.Clear();foreach(var x in rows)BalanceSheetRows.Add(x);OnPropertyChanged(nameof(BalanceAssets));OnPropertyChanged(nameof(BalanceLiabilities));OnPropertyChanged(nameof(BalanceEquity));OnPropertyChanged(nameof(BalanceDifference));OnPropertyChanged(nameof(BalanceIsBalanced));}catch(Exception ex){Message=ex.Message;}}

    private async Task LoadIncomeStatementAsync(){try{var rows=await _service.GetIncomeStatementAsync(_companyId,ReportFrom,ReportTo,PreviousReportFrom,PreviousReportTo);_incomeSummary=await _service.GetIncomeStatementSummaryAsync(_companyId,ReportFrom,ReportTo,PreviousReportFrom,PreviousReportTo);IncomeStatementRows.Clear();foreach(var x in rows)IncomeStatementRows.Add(x);OnPropertyChanged(nameof(GrossRevenue));OnPropertyChanged(nameof(NetRevenue));OnPropertyChanged(nameof(GrossProfit));OnPropertyChanged(nameof(OperatingResult));OnPropertyChanged(nameof(NetResult));OnPropertyChanged(nameof(PreviousNetResult));OnPropertyChanged(nameof(OperatingMargin));OnPropertyChanged(nameof(NetMargin));}catch(Exception ex){Message=ex.Message;}}
    private async Task SaveAsync(){Message=string.Empty;try{var lines=DraftLines.Select(x=>new SaveAccountingEntryLineRequest(x.Account?.Id??0,x.CostCenter?.Id,x.Description,x.Debit,x.Credit)).ToList();await _service.SaveDraftAsync(new SaveAccountingEntryRequest(_companyId,null,EntryDate,DocumentNumber.Trim(),Reference.Trim(),Description.Trim(),"Manual",_userId,_userName,lines));Message="Rascunho guardado com sucesso.";ResetDraft();await LoadAsync();}catch(Exception ex){Message=ex.Message;}}
    private async Task PostAsync(){if(SelectedEntry is null)return;try{await _service.PostAsync(_companyId,SelectedEntry.Id,_userId,_userName);Message="Lançamento contabilizado.";await LoadAsync();}catch(Exception ex){Message=ex.Message;}}
    private async Task ReverseAsync(){if(SelectedEntry is null)return;try{await _service.ReverseAsync(_companyId,SelectedEntry.Id,_userId,_userName);Message="Lançamento estornado.";await LoadAsync();}catch(Exception ex){Message=ex.Message;}}
    private void ResetDraft(){EntryDate=DateTime.Today;DocumentNumber=$"CTB-{DateTime.Now:yyyyMMdd-HHmmss}";Reference=string.Empty;Description=string.Empty;DraftLines.Clear();DraftLines.Add(new());DraftLines.Add(new());}
    private void ExportJournal()=>ExportadorCsv.Exportar($"diario-{ReportFrom:yyyyMMdd}-{ReportTo:yyyyMMdd}.csv",["Data","Documento","Referência","Conta","Descrição","Débito","Crédito","Estado"],JournalRows.Select(x=>(IReadOnlyList<string>)[x.EntryDate.ToString("dd/MM/yyyy"),x.DocumentNumber,x.Reference,$"{x.AccountCode} - {x.AccountName}",x.LineDescription,x.Debit.ToString("N2"),x.Credit.ToString("N2"),x.Status]));
    private void ExportLedger()=>ExportadorCsv.Exportar($"razao-{SelectedLedgerAccount?.Code}-{ReportFrom:yyyyMMdd}-{ReportTo:yyyyMMdd}.csv",["Data","Documento","Referência","Descrição","Débito","Crédito","Saldo"],LedgerRows.Select(x=>(IReadOnlyList<string>)[x.EntryDate.ToString("dd/MM/yyyy"),x.DocumentNumber,x.Reference,x.Description,x.Debit.ToString("N2"),x.Credit.ToString("N2"),x.RunningBalance.ToString("N2")]));
    private void ExportIncomeStatement()=>ExportadorCsv.Exportar($"dre-{ReportFrom:yyyyMMdd}-{ReportTo:yyyyMMdd}.csv",["Código","Conta","Grupo","Período atual","Período anterior","Variação"],IncomeStatementRows.Select(x=>(IReadOnlyList<string>)[x.Code,x.Description,x.LineType,x.CurrentAmount.ToString("N2"),x.PreviousAmount.ToString("N2"),x.Variation.ToString("N2")]));
    private void ExportBalanceSheet()=>ExportadorCsv.Exportar($"balanco-{ReportTo:yyyyMMdd}.csv",["Código","Conta","Secção","Atual","Comparativo","Variação"],BalanceSheetRows.Select(x=>(IReadOnlyList<string>)[x.AccountCode,x.AccountName,x.Section,x.CurrentAmount.ToString("N2"),x.PreviousAmount.ToString("N2"),x.Variation.ToString("N2")]));
    private void ExportTrialBalance()=>ExportadorCsv.Exportar($"balancete-{ReportFrom:yyyyMMdd}-{ReportTo:yyyyMMdd}.csv",["Código","Conta","Saldo inicial débito","Saldo inicial crédito","Débitos","Créditos","Saldo final débito","Saldo final crédito"],TrialBalanceRows.Select(x=>(IReadOnlyList<string>)[x.AccountCode,x.AccountName,x.OpeningDebit.ToString("N2"),x.OpeningCredit.ToString("N2"),x.PeriodDebit.ToString("N2"),x.PeriodCredit.ToString("N2"),x.ClosingDebit.ToString("N2"),x.ClosingCredit.ToString("N2")]));
}

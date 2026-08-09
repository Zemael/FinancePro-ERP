namespace FinancePro.Platform.Accounting;

public static class AccountingEntryStatus
{
    public const string Draft = "Rascunho";
    public const string Posted = "Contabilizado";
    public const string Reversed = "Estornado";
}

public sealed record AccountingEntryLine(
    int? Id,
    int AccountId,
    string AccountCode,
    string AccountName,
    int? CostCenterId,
    string? CostCenterCode,
    string Description,
    decimal Debit,
    decimal Credit);

public sealed record AccountingEntry(
    int Id,
    int CompanyId,
    DateTime EntryDate,
    string DocumentNumber,
    string Reference,
    string Description,
    string SourceModule,
    string Status,
    int CreatedBy,
    string CreatedByName,
    DateTime CreatedAt,
    IReadOnlyList<AccountingEntryLine> Lines)
{
    public decimal TotalDebit => Lines.Sum(x => x.Debit);
    public decimal TotalCredit => Lines.Sum(x => x.Credit);
    public bool IsBalanced => TotalDebit == TotalCredit;
}

public sealed record SaveAccountingEntryRequest(
    int CompanyId,
    int? Id,
    DateTime EntryDate,
    string DocumentNumber,
    string Reference,
    string Description,
    string SourceModule,
    int UserId,
    string UserName,
    IReadOnlyList<SaveAccountingEntryLineRequest> Lines);

public sealed record SaveAccountingEntryLineRequest(
    int AccountId,
    int? CostCenterId,
    string Description,
    decimal Debit,
    decimal Credit);

public sealed record AccountingEntrySummary(
    int Drafts,
    int Posted,
    decimal TotalDebit,
    decimal TotalCredit);

public sealed record GeneralJournalRow(
    int EntryId,
    DateTime EntryDate,
    string DocumentNumber,
    string Reference,
    string Description,
    string SourceModule,
    string Status,
    string AccountCode,
    string AccountName,
    string LineDescription,
    decimal Debit,
    decimal Credit);

public sealed record GeneralLedgerRow(
    DateTime EntryDate,
    string DocumentNumber,
    string Reference,
    string Description,
    decimal Debit,
    decimal Credit,
    decimal RunningBalance);

public sealed record TrialBalanceRow(
    int AccountId,
    string AccountCode,
    string AccountName,
    decimal OpeningDebit,
    decimal OpeningCredit,
    decimal PeriodDebit,
    decimal PeriodCredit,
    decimal ClosingDebit,
    decimal ClosingCredit);

public sealed record TrialBalanceSummary(
    decimal OpeningDebit,
    decimal OpeningCredit,
    decimal PeriodDebit,
    decimal PeriodCredit,
    decimal ClosingDebit,
    decimal ClosingCredit)
{
    public bool IsBalanced => PeriodDebit == PeriodCredit && ClosingDebit == ClosingCredit;
}


public static class IncomeStatementLineType
{
    public const string Revenue = "Receita";
    public const string Deduction = "Deducao";
    public const string Cost = "Custo";
    public const string OperatingExpense = "DespesaOperacional";
    public const string FinancialResult = "ResultadoFinanceiro";
    public const string Tax = "Imposto";
}

public sealed record IncomeStatementRow(
    string Code,
    string Description,
    string LineType,
    decimal CurrentAmount,
    decimal PreviousAmount,
    bool IsSubtotal = false)
{
    public decimal Variation => CurrentAmount - PreviousAmount;
}

public sealed record IncomeStatementSummary(
    decimal GrossRevenue,
    decimal Deductions,
    decimal NetRevenue,
    decimal Costs,
    decimal GrossProfit,
    decimal OperatingExpenses,
    decimal OperatingResult,
    decimal FinancialResult,
    decimal ResultBeforeTax,
    decimal Taxes,
    decimal NetResult,
    decimal PreviousNetResult)
{
    public decimal OperatingMargin => NetRevenue == 0 ? 0 : OperatingResult / NetRevenue * 100m;
    public decimal NetMargin => NetRevenue == 0 ? 0 : NetResult / NetRevenue * 100m;
}


public static class BalanceSheetSection
{
    public const string CurrentAsset = "AtivoCirculante";
    public const string NonCurrentAsset = "AtivoNaoCirculante";
    public const string CurrentLiability = "PassivoCirculante";
    public const string NonCurrentLiability = "PassivoNaoCirculante";
    public const string Equity = "PatrimonioLiquido";
}

public sealed record BalanceSheetRow(
    string AccountCode,
    string AccountName,
    string Section,
    decimal CurrentAmount,
    decimal PreviousAmount)
{
    public decimal Variation => CurrentAmount - PreviousAmount;
}

public sealed record BalanceSheetSummary(
    decimal CurrentAssets,
    decimal NonCurrentAssets,
    decimal CurrentLiabilities,
    decimal NonCurrentLiabilities,
    decimal Equity,
    decimal PreviousAssets,
    decimal PreviousLiabilitiesAndEquity)
{
    public decimal TotalAssets => CurrentAssets + NonCurrentAssets;
    public decimal TotalLiabilities => CurrentLiabilities + NonCurrentLiabilities;
    public decimal TotalLiabilitiesAndEquity => TotalLiabilities + Equity;
    public decimal Difference => TotalAssets - TotalLiabilitiesAndEquity;
    public bool IsBalanced => Math.Abs(Difference) < 0.01m;
}

public static class CashFlowMethod
{
    public const string Direct = "Direto";
    public const string Indirect = "Indireto";
}

public static class CashFlowActivity
{
    public const string Operating = "Operacional";
    public const string Investing = "Investimento";
    public const string Financing = "Financiamento";
}

public sealed record CashFlowRow(
    DateTime Date,
    string DocumentNumber,
    string Description,
    string Activity,
    decimal Inflow,
    decimal Outflow,
    bool IsSubtotal = false)
{
    public decimal NetAmount => Inflow - Outflow;
}

public sealed record CashFlowSummary(
    decimal OpeningBalance,
    decimal OperatingNet,
    decimal InvestingNet,
    decimal FinancingNet,
    decimal ClosingBalance)
{
    public decimal NetChange => OperatingNet + InvestingNet + FinancingNet;
    public bool IsReconciled => Math.Abs((OpeningBalance + NetChange) - ClosingBalance) < 0.01m;
}


public static class AccountingPeriodStatus
{
    public const string Open = "Aberto";
    public const string Closed = "Fechado";
    public const string Blocked = "Bloqueado";
}

public sealed record AccountingPeriodClosingCheck(string Code, string Description, bool IsBlocking, int Count, string Message);
public sealed record AccountingPeriodClosingPreview(int PeriodId, int FiscalYear, int Month, DateTime StartDate, DateTime EndDate, string Status, IReadOnlyList<AccountingPeriodClosingCheck> Checks)
{
    public bool CanClose => Status == AccountingPeriodStatus.Open && Checks.All(x => !x.IsBlocking || x.Count == 0);
    public int BlockingIssues => Checks.Where(x => x.IsBlocking).Sum(x => x.Count);
}
public sealed record AccountingPeriodClosingHistory(int Id, int PeriodId, string Operation, string PreviousStatus, string NewStatus, int UserId, string UserName, string Reason, DateTime CreatedAt);

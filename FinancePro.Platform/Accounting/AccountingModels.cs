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

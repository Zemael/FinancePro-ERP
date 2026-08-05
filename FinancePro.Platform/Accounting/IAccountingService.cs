namespace FinancePro.Platform.Accounting;

public interface IAccountingService
{
    Task<IReadOnlyList<AccountingEntry>> ListAsync(int companyId, DateTime? from = null, DateTime? to = null, CancellationToken cancellationToken = default);
    Task<AccountingEntrySummary> GetSummaryAsync(int companyId, CancellationToken cancellationToken = default);
    Task<int> SaveDraftAsync(SaveAccountingEntryRequest request, CancellationToken cancellationToken = default);
    Task PostAsync(int companyId, int id, int userId, string userName, CancellationToken cancellationToken = default);
    Task ReverseAsync(int companyId, int id, int userId, string userName, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<GeneralJournalRow>> GetGeneralJournalAsync(int companyId, DateTime from, DateTime to, string? status = null, string? search = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<GeneralLedgerRow>> GetGeneralLedgerAsync(int companyId, int accountId, DateTime from, DateTime to, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TrialBalanceRow>> GetTrialBalanceAsync(int companyId, DateTime from, DateTime to, CancellationToken cancellationToken = default);
    Task<TrialBalanceSummary> GetTrialBalanceSummaryAsync(int companyId, DateTime from, DateTime to, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<IncomeStatementRow>> GetIncomeStatementAsync(int companyId, DateTime from, DateTime to, DateTime previousFrom, DateTime previousTo, CancellationToken cancellationToken = default);
    Task<IncomeStatementSummary> GetIncomeStatementSummaryAsync(int companyId, DateTime from, DateTime to, DateTime previousFrom, DateTime previousTo, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BalanceSheetRow>> GetBalanceSheetAsync(int companyId, DateTime asOf, DateTime previousAsOf, CancellationToken cancellationToken = default);
    Task<BalanceSheetSummary> GetBalanceSheetSummaryAsync(int companyId, DateTime asOf, DateTime previousAsOf, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CashFlowRow>> GetCashFlowAsync(int companyId, DateTime from, DateTime to, string method, CancellationToken cancellationToken = default);
    Task<CashFlowSummary> GetCashFlowSummaryAsync(int companyId, DateTime from, DateTime to, string method, CancellationToken cancellationToken = default);
}

namespace FinancePro.Platform.Accounting;

public interface IAccountingStore
{
    Task<IReadOnlyList<AccountingEntry>> ListAsync(int companyId, DateTime? from = null, DateTime? to = null, CancellationToken cancellationToken = default);
    Task<AccountingEntry?> GetAsync(int companyId, int id, CancellationToken cancellationToken = default);
    Task<int> SaveAsync(SaveAccountingEntryRequest request, CancellationToken cancellationToken = default);
    Task SetStatusAsync(int companyId, int id, string status, int userId, string userName, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<GeneralJournalRow>> GetGeneralJournalAsync(int companyId, DateTime from, DateTime to, string? status = null, string? search = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<GeneralLedgerRow>> GetGeneralLedgerAsync(int companyId, int accountId, DateTime from, DateTime to, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TrialBalanceRow>> GetTrialBalanceAsync(int companyId, DateTime from, DateTime to, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<IncomeStatementRow>> GetIncomeStatementAsync(int companyId, DateTime from, DateTime to, DateTime previousFrom, DateTime previousTo, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BalanceSheetRow>> GetBalanceSheetAsync(int companyId, DateTime asOf, DateTime previousAsOf, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CashFlowRow>> GetDirectCashFlowAsync(int companyId, DateTime from, DateTime to, CancellationToken cancellationToken = default);
    Task<decimal> GetCashBalanceAsync(int companyId, DateTime asOf, CancellationToken cancellationToken = default);
}

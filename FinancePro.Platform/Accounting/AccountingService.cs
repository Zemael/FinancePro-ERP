namespace FinancePro.Platform.Accounting;

public sealed class AccountingService : IAccountingService
{
    private readonly IAccountingStore _store;
    public AccountingService(IAccountingStore store) => _store = store;

    public Task<IReadOnlyList<AccountingEntry>> ListAsync(int companyId, DateTime? from = null, DateTime? to = null, CancellationToken cancellationToken = default)
        => _store.ListAsync(companyId, from, to, cancellationToken);

    public async Task<AccountingEntrySummary> GetSummaryAsync(int companyId, CancellationToken cancellationToken = default)
    {
        var entries = await _store.ListAsync(companyId, null, null, cancellationToken);
        return new AccountingEntrySummary(
            entries.Count(x => x.Status == AccountingEntryStatus.Draft),
            entries.Count(x => x.Status == AccountingEntryStatus.Posted),
            entries.Where(x => x.Status != AccountingEntryStatus.Reversed).Sum(x => x.TotalDebit),
            entries.Where(x => x.Status != AccountingEntryStatus.Reversed).Sum(x => x.TotalCredit));
    }

    public Task<int> SaveDraftAsync(SaveAccountingEntryRequest request, CancellationToken cancellationToken = default)
    {
        Validate(request);
        return _store.SaveAsync(request, cancellationToken);
    }

    public async Task PostAsync(int companyId, int id, int userId, string userName, CancellationToken cancellationToken = default)
    {
        var entry = await RequiredAsync(companyId, id, cancellationToken);
        if (entry.Status != AccountingEntryStatus.Draft)
            throw new InvalidOperationException("Apenas lançamentos em rascunho podem ser contabilizados.");
        if (!entry.IsBalanced || entry.TotalDebit <= 0)
            throw new InvalidOperationException("O lançamento deve estar equilibrado e possuir valor superior a zero.");
        await _store.SetStatusAsync(companyId, id, AccountingEntryStatus.Posted, userId, userName, cancellationToken);
    }

    public async Task ReverseAsync(int companyId, int id, int userId, string userName, CancellationToken cancellationToken = default)
    {
        var entry = await RequiredAsync(companyId, id, cancellationToken);
        if (entry.Status != AccountingEntryStatus.Posted)
            throw new InvalidOperationException("Apenas lançamentos contabilizados podem ser estornados.");
        await _store.SetStatusAsync(companyId, id, AccountingEntryStatus.Reversed, userId, userName, cancellationToken);
    }

    public Task<IReadOnlyList<GeneralJournalRow>> GetGeneralJournalAsync(int companyId, DateTime from, DateTime to, string? status = null, string? search = null, CancellationToken cancellationToken = default)
    {
        ValidatePeriod(companyId, from, to);
        return _store.GetGeneralJournalAsync(companyId, from.Date, to.Date, status, search, cancellationToken);
    }

    public Task<IReadOnlyList<GeneralLedgerRow>> GetGeneralLedgerAsync(int companyId, int accountId, DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        ValidatePeriod(companyId, from, to);
        if (accountId <= 0) throw new ArgumentException("Conta contabilística obrigatória.");
        return _store.GetGeneralLedgerAsync(companyId, accountId, from.Date, to.Date, cancellationToken);
    }

    public Task<IReadOnlyList<TrialBalanceRow>> GetTrialBalanceAsync(int companyId, DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        ValidatePeriod(companyId, from, to);
        return _store.GetTrialBalanceAsync(companyId, from.Date, to.Date, cancellationToken);
    }

    public async Task<TrialBalanceSummary> GetTrialBalanceSummaryAsync(int companyId, DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        var rows = await GetTrialBalanceAsync(companyId, from, to, cancellationToken);
        return new TrialBalanceSummary(
            rows.Sum(x => x.OpeningDebit), rows.Sum(x => x.OpeningCredit),
            rows.Sum(x => x.PeriodDebit), rows.Sum(x => x.PeriodCredit),
            rows.Sum(x => x.ClosingDebit), rows.Sum(x => x.ClosingCredit));
    }


    public Task<IReadOnlyList<IncomeStatementRow>> GetIncomeStatementAsync(int companyId, DateTime from, DateTime to, DateTime previousFrom, DateTime previousTo, CancellationToken cancellationToken = default)
    {
        ValidatePeriod(companyId, from, to);
        ValidatePeriod(companyId, previousFrom, previousTo);
        return _store.GetIncomeStatementAsync(companyId, from.Date, to.Date, previousFrom.Date, previousTo.Date, cancellationToken);
    }

    public async Task<IncomeStatementSummary> GetIncomeStatementSummaryAsync(int companyId, DateTime from, DateTime to, DateTime previousFrom, DateTime previousTo, CancellationToken cancellationToken = default)
    {
        var rows = await GetIncomeStatementAsync(companyId, from, to, previousFrom, previousTo, cancellationToken);
        decimal Current(string type) => rows.Where(x => !x.IsSubtotal && x.LineType == type).Sum(x => x.CurrentAmount);
        decimal Previous(string type) => rows.Where(x => !x.IsSubtotal && x.LineType == type).Sum(x => x.PreviousAmount);

        var grossRevenue = Current(IncomeStatementLineType.Revenue);
        var deductions = Current(IncomeStatementLineType.Deduction);
        var netRevenue = grossRevenue - deductions;
        var costs = Current(IncomeStatementLineType.Cost);
        var grossProfit = netRevenue - costs;
        var operatingExpenses = Current(IncomeStatementLineType.OperatingExpense);
        var operatingResult = grossProfit - operatingExpenses;
        var financialResult = Current(IncomeStatementLineType.FinancialResult);
        var resultBeforeTax = operatingResult + financialResult;
        var taxes = Current(IncomeStatementLineType.Tax);
        var netResult = resultBeforeTax - taxes;

        var previousNetRevenue = Previous(IncomeStatementLineType.Revenue) - Previous(IncomeStatementLineType.Deduction);
        var previousNetResult = previousNetRevenue - Previous(IncomeStatementLineType.Cost) - Previous(IncomeStatementLineType.OperatingExpense) + Previous(IncomeStatementLineType.FinancialResult) - Previous(IncomeStatementLineType.Tax);
        return new IncomeStatementSummary(grossRevenue, deductions, netRevenue, costs, grossProfit, operatingExpenses, operatingResult, financialResult, resultBeforeTax, taxes, netResult, previousNetResult);
    }

    public Task<IReadOnlyList<BalanceSheetRow>> GetBalanceSheetAsync(int companyId, DateTime asOf, DateTime previousAsOf, CancellationToken cancellationToken = default)
    {
        if (companyId <= 0) throw new ArgumentException("Empresa inválida.");
        if (previousAsOf.Date > asOf.Date) throw new ArgumentException("A data comparativa não pode ser posterior à data atual.");
        return _store.GetBalanceSheetAsync(companyId, asOf.Date, previousAsOf.Date, cancellationToken);
    }

    public async Task<BalanceSheetSummary> GetBalanceSheetSummaryAsync(int companyId, DateTime asOf, DateTime previousAsOf, CancellationToken cancellationToken = default)
    {
        var rows = await GetBalanceSheetAsync(companyId, asOf, previousAsOf, cancellationToken);
        decimal Current(string section) => rows.Where(x => x.Section == section).Sum(x => x.CurrentAmount);
        decimal Previous(string section) => rows.Where(x => x.Section == section).Sum(x => x.PreviousAmount);
        return new BalanceSheetSummary(
            Current(BalanceSheetSection.CurrentAsset), Current(BalanceSheetSection.NonCurrentAsset),
            Current(BalanceSheetSection.CurrentLiability), Current(BalanceSheetSection.NonCurrentLiability),
            Current(BalanceSheetSection.Equity),
            Previous(BalanceSheetSection.CurrentAsset) + Previous(BalanceSheetSection.NonCurrentAsset),
            Previous(BalanceSheetSection.CurrentLiability) + Previous(BalanceSheetSection.NonCurrentLiability) + Previous(BalanceSheetSection.Equity));
    }

    private static void ValidatePeriod(int companyId, DateTime from, DateTime to)
    {
        if (companyId <= 0) throw new ArgumentException("Empresa inválida.");
        if (from.Date > to.Date) throw new ArgumentException("A data inicial não pode ser superior à data final.");
    }

    private async Task<AccountingEntry> RequiredAsync(int companyId, int id, CancellationToken cancellationToken)
        => await _store.GetAsync(companyId, id, cancellationToken)
           ?? throw new KeyNotFoundException("Lançamento contabilístico não encontrado.");

    private static void Validate(SaveAccountingEntryRequest request)
    {
        if (request.CompanyId <= 0) throw new ArgumentException("Empresa inválida.");
        if (request.UserId <= 0) throw new ArgumentException("Utilizador inválido.");
        if (string.IsNullOrWhiteSpace(request.Description)) throw new ArgumentException("Descrição obrigatória.");
        if (string.IsNullOrWhiteSpace(request.DocumentNumber)) throw new ArgumentException("Número do documento obrigatório.");
        if (request.Lines.Count < 2) throw new ArgumentException("O lançamento deve possuir pelo menos duas linhas.");

        foreach (var line in request.Lines)
        {
            if (line.AccountId <= 0) throw new ArgumentException("Conta contabilística inválida.");
            if (line.Debit < 0 || line.Credit < 0) throw new ArgumentException("Débito e crédito não podem ser negativos.");
            if ((line.Debit > 0 && line.Credit > 0) || (line.Debit == 0 && line.Credit == 0))
                throw new ArgumentException("Cada linha deve possuir apenas débito ou apenas crédito.");
        }

        var debit = request.Lines.Sum(x => x.Debit);
        var credit = request.Lines.Sum(x => x.Credit);
        if (debit <= 0 || debit != credit)
            throw new ArgumentException("O total de débitos deve ser igual ao total de créditos.");
    }
}

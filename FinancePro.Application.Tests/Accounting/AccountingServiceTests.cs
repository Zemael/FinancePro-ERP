using FinancePro.Platform.Accounting;
using Xunit;

namespace FinancePro.Application.Tests.Accounting;

public sealed class AccountingServiceTests
{
    [Fact]
    public async Task SaveDraft_rejects_unbalanced_entry()
    {
        var service = new AccountingService(new FakeStore());
        var request = new SaveAccountingEntryRequest(1,null,DateTime.Today,"CTB-1","","Teste","Manual",1,"Admin",
            [new(1,null,"Débito",100,0),new(2,null,"Crédito",0,90)]);
        await Assert.ThrowsAsync<ArgumentException>(() => service.SaveDraftAsync(request));
    }

    [Fact]
    public async Task Post_accepts_balanced_draft()
    {
        var store = new FakeStore(); var service = new AccountingService(store);
        await service.PostAsync(1,1,1,"Admin");
        Assert.Equal(AccountingEntryStatus.Posted, store.Status);
    }


    [Fact]
    public async Task Trial_balance_summary_is_balanced()
    {
        var service = new AccountingService(new FakeStore());
        var summary = await service.GetTrialBalanceSummaryAsync(1, DateTime.Today.AddDays(-1), DateTime.Today);
        Assert.True(summary.IsBalanced);
        Assert.Equal(100m, summary.PeriodDebit);
        Assert.Equal(100m, summary.PeriodCredit);
    }

    [Fact]
    public async Task Ledger_requires_account()
    {
        var service = new AccountingService(new FakeStore());
        await Assert.ThrowsAsync<ArgumentException>(() => service.GetGeneralLedgerAsync(1, 0, DateTime.Today, DateTime.Today));
    }


    [Fact]
    public async Task Income_statement_calculates_net_result()
    {
        var service = new AccountingService(new FakeStore());
        var summary = await service.GetIncomeStatementSummaryAsync(1, DateTime.Today.AddMonths(-1), DateTime.Today, DateTime.Today.AddMonths(-2), DateTime.Today.AddMonths(-1).AddDays(-1));
        Assert.Equal(35m, summary.NetResult);
        Assert.Equal(100m, summary.GrossRevenue);
    }

    private sealed class FakeStore : IAccountingStore
    {
        public string Status { get; private set; } = AccountingEntryStatus.Draft;
        private AccountingEntry Entry => new(1,1,DateTime.Today,"CTB-1","","Teste","Manual",Status,1,"Admin",DateTime.UtcNow,
            [new(null,1,"1","Caixa",null,null,"",100,0),new(null,2,"2","Capital",null,null,"",0,100)]);
        public Task<IReadOnlyList<AccountingEntry>> ListAsync(int companyId,DateTime? from=null,DateTime? to=null,CancellationToken cancellationToken=default)=>Task.FromResult<IReadOnlyList<AccountingEntry>>([Entry]);
        public Task<AccountingEntry?> GetAsync(int companyId,int id,CancellationToken cancellationToken=default)=>Task.FromResult<AccountingEntry?>(Entry);
        public Task<int> SaveAsync(SaveAccountingEntryRequest request,CancellationToken cancellationToken=default)=>Task.FromResult(1);
        public Task SetStatusAsync(int companyId,int id,string status,int userId,string userName,CancellationToken cancellationToken=default){Status=status;return Task.CompletedTask;}
        public Task<IReadOnlyList<GeneralJournalRow>> GetGeneralJournalAsync(int companyId,DateTime from,DateTime to,string? status=null,string? search=null,CancellationToken cancellationToken=default)=>Task.FromResult<IReadOnlyList<GeneralJournalRow>>([new(1,DateTime.Today,"CTB-1","","Teste","Manual",Status,"1","Caixa","",100,0),new(1,DateTime.Today,"CTB-1","","Teste","Manual",Status,"2","Capital","",0,100)]);
        public Task<IReadOnlyList<GeneralLedgerRow>> GetGeneralLedgerAsync(int companyId,int accountId,DateTime from,DateTime to,CancellationToken cancellationToken=default)=>Task.FromResult<IReadOnlyList<GeneralLedgerRow>>([new(DateTime.Today,"CTB-1","","Teste",100,0,100)]);

        public Task<IReadOnlyList<IncomeStatementRow>> GetIncomeStatementAsync(int companyId,DateTime from,DateTime to,DateTime previousFrom,DateTime previousTo,CancellationToken cancellationToken=default)
            => Task.FromResult<IReadOnlyList<IncomeStatementRow>>([
                new("4000","Receitas",IncomeStatementLineType.Revenue,100,90),
                new("5000","Custos",IncomeStatementLineType.Cost,40,35),
                new("6000","Despesas",IncomeStatementLineType.OperatingExpense,20,18),
                new("7000","Resultado financeiro",IncomeStatementLineType.FinancialResult,5,4),
                new("8000","Impostos",IncomeStatementLineType.Tax,10,8)]);
        public Task<IReadOnlyList<TrialBalanceRow>> GetTrialBalanceAsync(int companyId,DateTime from,DateTime to,CancellationToken cancellationToken=default)=>Task.FromResult<IReadOnlyList<TrialBalanceRow>>([new(1,"1","Caixa",0,0,100,0,100,0),new(2,"2","Capital",0,0,0,100,0,100)]);
    }
}

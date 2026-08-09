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


    [Fact]
    public async Task Balance_sheet_is_balanced()
    {
        var service = new AccountingService(new FakeStore());
        var summary = await service.GetBalanceSheetSummaryAsync(1, DateTime.Today, DateTime.Today.AddYears(-1));
        Assert.True(summary.IsBalanced);
        Assert.Equal(150m, summary.TotalAssets);
    }


    [Fact]
    public async Task Cash_flow_reconciles_opening_and_closing_balances()
    {
        var service = new AccountingService(new FakeStore());
        var summary = await service.GetCashFlowSummaryAsync(1, DateTime.Today.AddDays(-5), DateTime.Today, CashFlowMethod.Direct);
        Assert.True(summary.IsReconciled);
        Assert.Equal(30m, summary.NetChange);
    }


    [Fact]
    public async Task Close_period_rejects_blocking_drafts()
    {
        var store = new FakeStore { ClosingDrafts = 2 }; var service = new AccountingService(store);
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.ClosePeriodAsync(1, 1, 1, "Admin"));
    }

    [Fact]
    public async Task Close_and_reopen_period_are_audited()
    {
        var store = new FakeStore(); var service = new AccountingService(store);
        await service.ClosePeriodAsync(1, 1, 1, "Admin");
        Assert.Equal("Fechado", store.PeriodStatus);
        await service.ReopenPeriodAsync(1, 1, 1, "Admin", "Correção autorizada");
        Assert.Equal("Aberto", store.PeriodStatus);
        Assert.Equal(2, store.History.Count);
    }

    [Fact]
    public async Task Save_draft_rejects_closed_period()
    {
        var store = new FakeStore { DateOpen = false }; var service = new AccountingService(store);
        var request = new SaveAccountingEntryRequest(1,null,DateTime.Today,"CTB-2","","Teste","Manual",1,"Admin",[new(1,null,"D",100,0),new(2,null,"C",0,100)]);
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.SaveDraftAsync(request));
    }

    private sealed class FakeStore : IAccountingStore
    {
        public string Status { get; private set; } = AccountingEntryStatus.Draft;
        public string PeriodStatus { get; private set; } = AccountingPeriodStatus.Open;
        public int ClosingDrafts { get; set; }
        public bool DateOpen { get; set; } = true;
        public List<AccountingPeriodClosingHistory> History { get; } = [];
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
        public Task<IReadOnlyList<BalanceSheetRow>> GetBalanceSheetAsync(int companyId,DateTime asOf,DateTime previousAsOf,CancellationToken cancellationToken=default)
            => Task.FromResult<IReadOnlyList<BalanceSheetRow>>([
                new("1000","Caixa",BalanceSheetSection.CurrentAsset,100,80),
                new("1500","Equipamentos",BalanceSheetSection.NonCurrentAsset,50,55),
                new("2000","Fornecedores",BalanceSheetSection.CurrentLiability,40,35),
                new("2500","Empréstimos",BalanceSheetSection.NonCurrentLiability,30,35),
                new("3000","Capital",BalanceSheetSection.Equity,80,65)]);

        public Task<IReadOnlyList<CashFlowRow>> GetDirectCashFlowAsync(int companyId,DateTime from,DateTime to,CancellationToken cancellationToken=default)
            => Task.FromResult<IReadOnlyList<CashFlowRow>>([
                new(DateTime.Today,"TES-1","Recebimento",CashFlowActivity.Operating,100,0),
                new(DateTime.Today,"TES-2","Pagamento",CashFlowActivity.Operating,0,50),
                new(DateTime.Today,"PAT-1","Aquisição",CashFlowActivity.Investing,0,20)]);
        public Task<decimal> GetCashBalanceAsync(int companyId,DateTime asOf,CancellationToken cancellationToken=default)
            => Task.FromResult(asOf.Date < DateTime.Today ? 70m : 100m);
        public Task<bool> IsDateOpenForPostingAsync(int companyId,DateTime date,CancellationToken cancellationToken=default)=>Task.FromResult(DateOpen);
        public Task<AccountingPeriodClosingPreview?> GetPeriodClosingPreviewAsync(int companyId,int periodId,CancellationToken cancellationToken=default)=>Task.FromResult<AccountingPeriodClosingPreview?>(new(periodId,2026,8,new DateTime(2026,8,1),new DateTime(2026,8,31),PeriodStatus,[new("DRAFT_ENTRIES","Rascunhos",true,ClosingDrafts,"")]));
        public Task ClosePeriodAsync(int companyId,int periodId,int userId,string userName,CancellationToken cancellationToken=default){var old=PeriodStatus;PeriodStatus=AccountingPeriodStatus.Closed;History.Add(new(History.Count+1,periodId,"Fecho",old,PeriodStatus,userId,userName,"Fecho",DateTime.UtcNow));return Task.CompletedTask;}
        public Task ReopenPeriodAsync(int companyId,int periodId,int userId,string userName,string reason,CancellationToken cancellationToken=default){var old=PeriodStatus;PeriodStatus=AccountingPeriodStatus.Open;History.Add(new(History.Count+1,periodId,"Reabertura",old,PeriodStatus,userId,userName,reason,DateTime.UtcNow));return Task.CompletedTask;}
        public Task<IReadOnlyList<AccountingPeriodClosingHistory>> GetPeriodClosingHistoryAsync(int companyId,int periodId,CancellationToken cancellationToken=default)=>Task.FromResult<IReadOnlyList<AccountingPeriodClosingHistory>>(History);
        public Task<IReadOnlyList<TrialBalanceRow>> GetTrialBalanceAsync(int companyId,DateTime from,DateTime to,CancellationToken cancellationToken=default)=>Task.FromResult<IReadOnlyList<TrialBalanceRow>>([new(1,"1","Caixa",0,0,100,0,100,0),new(2,"2","Capital",0,0,0,100,0,100)]);
    }
}

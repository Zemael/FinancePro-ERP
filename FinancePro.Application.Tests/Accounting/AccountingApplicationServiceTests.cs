using FinancePro.Application.Accounting;
using FinancePro.Core.DTOs;
using FinancePro.Core.Enums;
using Xunit;

namespace FinancePro.Application.Tests.Accounting;

public sealed class AccountingApplicationServiceTests
{
    [Fact] public async Task Entry_must_balance_debits_and_credits(){var service=new AccountingApplicationService(new FakeGateway());var result=await service.SaveEntryAsync(new JournalEntryRequest(1,DateTime.Today,"Teste",null,null,null,new[]{new JournalLineRequest(1,null,100,0,null),new JournalLineRequest(2,null,0,90,null)}));Assert.False(result.IsSuccess);}
    [Fact] public async Task Balanced_entry_is_accepted(){var service=new AccountingApplicationService(new FakeGateway());var result=await service.SaveEntryAsync(new JournalEntryRequest(1,DateTime.Today,"Teste",null,null,null,new[]{new JournalLineRequest(1,null,100,0,null),new JournalLineRequest(2,null,0,100,null)}));Assert.True(result.IsSuccess);}
    private sealed class FakeGateway:IAccountingGateway{public Task<bool> AccountCodeExistsAsync(int a,string b,int c,CancellationToken d=default)=>Task.FromResult(false);public Task<IReadOnlyList<PlanoContaDto>> ListAccountsAsync(int a,string? b,CancellationToken c=default)=>Task.FromResult<IReadOnlyList<PlanoContaDto>>(Array.Empty<PlanoContaDto>());public Task<IReadOnlyList<LancamentoContabilDto>> ListEntriesAsync(int a,CancellationToken b=default)=>Task.FromResult<IReadOnlyList<LancamentoContabilDto>>(Array.Empty<LancamentoContabilDto>());public Task<int> SaveAccountAsync(AccountSaveRequest a,CancellationToken b=default)=>Task.FromResult(1);public Task<int> SaveEntryAsync(JournalEntryRequest a,CancellationToken b=default)=>Task.FromResult(1);public Task SetAccountActiveAsync(int a,bool b,CancellationToken c=default)=>Task.CompletedTask;public Task PostEntryAsync(int a,CancellationToken b=default)=>Task.CompletedTask;}
}

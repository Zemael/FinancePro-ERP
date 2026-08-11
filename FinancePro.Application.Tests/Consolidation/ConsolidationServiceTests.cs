using FinancePro.Platform.Consolidation;
using Xunit;

namespace FinancePro.Application.Tests.Consolidation;

public sealed class ConsolidationServiceTests
{
    [Fact]
    public async Task SaveGroup_RequiresAtLeastTwoCompanies()
    {
        var service = new ConsolidationService(new FakeStore());
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.SaveGroupAsync(new(null,"Grupo",2026,[1])));
    }

    [Fact]
    public async Task Validate_WhenBalancedAndReconciled_MarksRunAsValidated()
    {
        var store = new FakeStore(); var service = new ConsolidationService(store);
        var result = await service.ValidateAsync(1,10,"Admin");
        Assert.True(result.CanValidate);
        Assert.Equal(ConsolidationRunStatus.Validated, store.Run.Status);
    }

    [Fact]
    public async Task Consolidate_RequiresValidatedRun()
    {
        var store = new FakeStore(); var service = new ConsolidationService(store);
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.ConsolidateAsync(1,10,"Admin"));
    }

    private sealed class FakeStore : IConsolidationStore
    {
        public ConsolidationRun Run { get; private set; } = new(1,1,"Grupo",new DateTime(2026,1,1),new DateTime(2026,12,31),ConsolidationRunStatus.Draft,10,"Admin",DateTime.UtcNow,null,null,null);
        public Task<IReadOnlyList<ConsolidationCompany>> ListCompaniesAsync(CancellationToken cancellationToken=default) => Task.FromResult<IReadOnlyList<ConsolidationCompany>>([new(1,"A"),new(2,"B")]);
        public Task<IReadOnlyList<ConsolidationGroup>> ListGroupsAsync(CancellationToken cancellationToken=default) => Task.FromResult<IReadOnlyList<ConsolidationGroup>>([new(1,"Grupo",2026,[1,2],DateTime.UtcNow,DateTime.UtcNow)]);
        public Task<int> SaveGroupAsync(SaveConsolidationGroupRequest request,CancellationToken cancellationToken=default) => Task.FromResult(1);
        public Task<IReadOnlyList<ConsolidationRun>> ListRunsAsync(int groupId,CancellationToken cancellationToken=default) => Task.FromResult<IReadOnlyList<ConsolidationRun>>([Run]);
        public Task<ConsolidationRun?> GetRunAsync(int runId,CancellationToken cancellationToken=default) => Task.FromResult<ConsolidationRun?>(Run);
        public Task<int> CreateRunAsync(CreateConsolidationRunRequest request,CancellationToken cancellationToken=default) => Task.FromResult(1);
        public Task<IReadOnlyList<ConsolidatedTrialBalanceRow>> GetTrialBalanceAsync(int runId,CancellationToken cancellationToken=default) => Task.FromResult<IReadOnlyList<ConsolidatedTrialBalanceRow>>([new("100","Caixa",100,100,0,0,100,100)]);
        public Task<IReadOnlyList<IntercompanyDifference>> GetIntercompanyDifferencesAsync(int runId,CancellationToken cancellationToken=default) => Task.FromResult<IReadOnlyList<IntercompanyDifference>>([new("IC-001",2,"A, B",100,100,0)]);
        public Task<IReadOnlyList<ConsolidationElimination>> ListEliminationsAsync(int runId,CancellationToken cancellationToken=default) => Task.FromResult<IReadOnlyList<ConsolidationElimination>>([]);
        public Task<int> SaveEliminationAsync(SaveConsolidationEliminationRequest request,CancellationToken cancellationToken=default) => Task.FromResult(1);
        public Task DeleteEliminationAsync(int runId,int eliminationId,CancellationToken cancellationToken=default) => Task.CompletedTask;
        public Task SetRunStatusAsync(int runId,string status,int userId,string userName,CancellationToken cancellationToken=default){Run=Run with { Status=status };return Task.CompletedTask;}
    }
}

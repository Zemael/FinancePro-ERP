using FinancePro.Platform.Workflow;
using Xunit;

namespace FinancePro.Application.Tests.Workflow;

public sealed class WorkflowServiceTests
{
    [Fact]
    public async Task SubmitAsync_NormalizesAndCreatesTask()
    {
        var store = new FakeStore();
        var service = new WorkflowService(store);
        var id = await service.SubmitAsync(new CreateWorkflowTaskRequest(1," Compras ","Pedido"," CMP-1 "," Aprovar compra ",null,1000,2," Gestor ",1," Admin "));
        Assert.Equal(1, id);
        Assert.Equal("Compras", store.Created!.Module);
    }

    [Fact]
    public async Task RejectAsync_RequiresComment()
    {
        var store = new FakeStore();
        var service = new WorkflowService(store);
        await Assert.ThrowsAsync<ArgumentException>(() => service.RejectAsync(new WorkflowTaskDecision(1,1,2,"Gestor",WorkflowTaskStatus.Rejected,"")));
    }

    private sealed class FakeStore : IWorkflowStore
    {
        public CreateWorkflowTaskRequest? Created { get; private set; }
        public Task<long> CreateTaskAsync(CreateWorkflowTaskRequest request, CancellationToken cancellationToken = default) { Created=request; return Task.FromResult(1L); }
        public Task<IReadOnlyList<WorkflowTaskItem>> ListTasksAsync(int companyId, int userId, WorkflowTaskStatus? status = null, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<WorkflowTaskItem>>([]);
        public Task<WorkflowTaskItem?> FindTaskAsync(long taskId, int companyId, CancellationToken cancellationToken = default) => Task.FromResult<WorkflowTaskItem?>(new(taskId,companyId,"Compras","Pedido","CMP-1","Aprovar",null,100,2,"Gestor",WorkflowTaskStatus.Pending,WorkflowPriority.Normal,DateTime.UtcNow,null,"Admin",null));
        public Task DecideAsync(WorkflowTaskDecision decision, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<WorkflowSummary> GetSummaryAsync(int companyId, int userId, CancellationToken cancellationToken = default) => Task.FromResult(new WorkflowSummary(0,0,0,0));
    }
}

namespace FinancePro.Platform.Workflow;

public interface IWorkflowStore
{
    Task<long> CreateTaskAsync(CreateWorkflowTaskRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<WorkflowTaskItem>> ListTasksAsync(int companyId, int userId, WorkflowTaskStatus? status = null, CancellationToken cancellationToken = default);
    Task<WorkflowTaskItem?> FindTaskAsync(long taskId, int companyId, CancellationToken cancellationToken = default);
    Task DecideAsync(WorkflowTaskDecision decision, CancellationToken cancellationToken = default);
    Task<WorkflowSummary> GetSummaryAsync(int companyId, int userId, CancellationToken cancellationToken = default);
}

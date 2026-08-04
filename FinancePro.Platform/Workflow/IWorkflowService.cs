namespace FinancePro.Platform.Workflow;

public interface IWorkflowService
{
    Task<long> SubmitAsync(CreateWorkflowTaskRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<WorkflowTaskItem>> GetMyTasksAsync(int companyId, int userId, WorkflowTaskStatus? status = WorkflowTaskStatus.Pending, CancellationToken cancellationToken = default);
    Task<WorkflowSummary> GetSummaryAsync(int companyId, int userId, CancellationToken cancellationToken = default);
    Task ApproveAsync(WorkflowTaskDecision decision, CancellationToken cancellationToken = default);
    Task RejectAsync(WorkflowTaskDecision decision, CancellationToken cancellationToken = default);
    Task ReturnAsync(WorkflowTaskDecision decision, CancellationToken cancellationToken = default);
}

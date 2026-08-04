namespace FinancePro.Platform.Workflow;

public sealed class WorkflowService : IWorkflowService
{
    private readonly IWorkflowStore _store;

    public WorkflowService(IWorkflowStore store) => _store = store;

    public Task<long> SubmitAsync(CreateWorkflowTaskRequest request, CancellationToken cancellationToken = default)
    {
        ValidateRequest(request);
        return _store.CreateTaskAsync(request with
        {
            Module = request.Module.Trim(),
            DocumentType = request.DocumentType.Trim(),
            DocumentReference = request.DocumentReference.Trim(),
            Title = request.Title.Trim(),
            AssignedUserName = request.AssignedUserName.Trim(),
            RequestedByName = request.RequestedByName.Trim()
        }, cancellationToken);
    }

    public Task<IReadOnlyList<WorkflowTaskItem>> GetMyTasksAsync(int companyId, int userId, WorkflowTaskStatus? status = WorkflowTaskStatus.Pending, CancellationToken cancellationToken = default)
    {
        if (companyId <= 0) throw new ArgumentOutOfRangeException(nameof(companyId));
        if (userId <= 0) throw new ArgumentOutOfRangeException(nameof(userId));
        return _store.ListTasksAsync(companyId, userId, status, cancellationToken);
    }

    public Task<WorkflowSummary> GetSummaryAsync(int companyId, int userId, CancellationToken cancellationToken = default)
    {
        if (companyId <= 0) throw new ArgumentOutOfRangeException(nameof(companyId));
        if (userId <= 0) throw new ArgumentOutOfRangeException(nameof(userId));
        return _store.GetSummaryAsync(companyId, userId, cancellationToken);
    }

    public Task ApproveAsync(WorkflowTaskDecision decision, CancellationToken cancellationToken = default) =>
        DecideAsync(decision with { Decision = WorkflowTaskStatus.Approved }, false, cancellationToken);

    public Task RejectAsync(WorkflowTaskDecision decision, CancellationToken cancellationToken = default) =>
        DecideAsync(decision with { Decision = WorkflowTaskStatus.Rejected }, true, cancellationToken);

    public Task ReturnAsync(WorkflowTaskDecision decision, CancellationToken cancellationToken = default) =>
        DecideAsync(decision with { Decision = WorkflowTaskStatus.Returned }, true, cancellationToken);

    private async Task DecideAsync(WorkflowTaskDecision decision, bool commentRequired, CancellationToken cancellationToken)
    {
        if (decision.TaskId <= 0) throw new ArgumentOutOfRangeException(nameof(decision.TaskId));
        if (decision.CompanyId <= 0) throw new ArgumentOutOfRangeException(nameof(decision.CompanyId));
        if (decision.UserId <= 0) throw new ArgumentOutOfRangeException(nameof(decision.UserId));
        if (commentRequired && string.IsNullOrWhiteSpace(decision.Comment))
            throw new ArgumentException("A observação é obrigatória para rejeitar ou devolver uma tarefa.", nameof(decision));

        var task = await _store.FindTaskAsync(decision.TaskId, decision.CompanyId, cancellationToken)
            ?? throw new InvalidOperationException("Tarefa de workflow não encontrada.");

        if (task.AssignedUserId != decision.UserId)
            throw new InvalidOperationException("A tarefa pertence a outro utilizador.");
        if (task.Status != WorkflowTaskStatus.Pending)
            throw new InvalidOperationException("A tarefa já foi decidida.");

        await _store.DecideAsync(decision with
        {
            UserName = decision.UserName.Trim(),
            Comment = string.IsNullOrWhiteSpace(decision.Comment) ? null : decision.Comment.Trim()
        }, cancellationToken);
    }

    private static void ValidateRequest(CreateWorkflowTaskRequest request)
    {
        if (request.CompanyId <= 0) throw new ArgumentOutOfRangeException(nameof(request.CompanyId));
        if (request.AssignedUserId <= 0) throw new ArgumentOutOfRangeException(nameof(request.AssignedUserId));
        if (request.RequestedByUserId <= 0) throw new ArgumentOutOfRangeException(nameof(request.RequestedByUserId));
        if (string.IsNullOrWhiteSpace(request.Module)) throw new ArgumentException("Módulo obrigatório.", nameof(request));
        if (string.IsNullOrWhiteSpace(request.DocumentType)) throw new ArgumentException("Tipo de documento obrigatório.", nameof(request));
        if (string.IsNullOrWhiteSpace(request.DocumentReference)) throw new ArgumentException("Referência obrigatória.", nameof(request));
        if (string.IsNullOrWhiteSpace(request.Title)) throw new ArgumentException("Título obrigatório.", nameof(request));
        if (string.IsNullOrWhiteSpace(request.AssignedUserName)) throw new ArgumentException("Responsável obrigatório.", nameof(request));
    }
}

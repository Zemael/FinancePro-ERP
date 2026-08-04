namespace FinancePro.Platform.Workflow;

public enum WorkflowTaskStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2,
    Returned = 3,
    Delegated = 4,
    Cancelled = 5
}

public enum WorkflowPriority
{
    Normal = 0,
    High = 1,
    Urgent = 2,
    Critical = 3
}

public sealed record WorkflowTaskItem(
    long Id,
    int CompanyId,
    string Module,
    string DocumentType,
    string DocumentReference,
    string Title,
    string? Description,
    decimal? Amount,
    int AssignedUserId,
    string AssignedUserName,
    WorkflowTaskStatus Status,
    WorkflowPriority Priority,
    DateTime CreatedAt,
    DateTime? DueAt,
    string? RequestedByName,
    string? LastComment)
{
    public bool IsOverdue => Status == WorkflowTaskStatus.Pending && DueAt.HasValue && DueAt.Value < DateTime.UtcNow;
}

public sealed record WorkflowTaskDecision(
    long TaskId,
    int CompanyId,
    int UserId,
    string UserName,
    WorkflowTaskStatus Decision,
    string? Comment);

public sealed record CreateWorkflowTaskRequest(
    int CompanyId,
    string Module,
    string DocumentType,
    string DocumentReference,
    string Title,
    string? Description,
    decimal? Amount,
    int AssignedUserId,
    string AssignedUserName,
    int RequestedByUserId,
    string RequestedByName,
    WorkflowPriority Priority = WorkflowPriority.Normal,
    DateTime? DueAt = null);

public sealed record WorkflowSummary(int Pending, int Overdue, int Urgent, int CompletedToday);

using System.Data;
using System.Data.Common;
using FinancePro.Data.Context;
using FinancePro.Platform.Workflow;
using Microsoft.EntityFrameworkCore;

namespace FinancePro.Data.Workflow;

public sealed class SqlWorkflowStore : IWorkflowStore
{
    private readonly FinanceProDbContext _dbContext;
    public SqlWorkflowStore(FinanceProDbContext dbContext) => _dbContext = dbContext;

    public async Task<long> CreateTaskAsync(CreateWorkflowTaskRequest request, CancellationToken cancellationToken = default)
    {
        var connection = _dbContext.Database.GetDbConnection();
        var close = connection.State != ConnectionState.Open;
        if (close) await connection.OpenAsync(cancellationToken);
        try
        {
            await using var command = connection.CreateCommand();
            command.CommandText = @"INSERT INTO dbo.WorkflowTasks
(CompanyId, Module, DocumentType, DocumentReference, Title, Description, Amount, AssignedUserId, AssignedUserName, RequestedByUserId, RequestedByName, Status, Priority, CreatedAt, DueAt)
OUTPUT INSERTED.Id
VALUES (@companyId,@module,@documentType,@documentReference,@title,@description,@amount,@assignedUserId,@assignedUserName,@requestedByUserId,@requestedByName,0,@priority,SYSUTCDATETIME(),@dueAt);";
            Add(command, "@companyId", request.CompanyId); Add(command, "@module", request.Module);
            Add(command, "@documentType", request.DocumentType); Add(command, "@documentReference", request.DocumentReference);
            Add(command, "@title", request.Title); Add(command, "@description", request.Description); Add(command, "@amount", request.Amount);
            Add(command, "@assignedUserId", request.AssignedUserId); Add(command, "@assignedUserName", request.AssignedUserName);
            Add(command, "@requestedByUserId", request.RequestedByUserId); Add(command, "@requestedByName", request.RequestedByName);
            Add(command, "@priority", (int)request.Priority); Add(command, "@dueAt", request.DueAt);
            return Convert.ToInt64(await command.ExecuteScalarAsync(cancellationToken));
        }
        finally { if (close) await connection.CloseAsync(); }
    }

    public async Task<IReadOnlyList<WorkflowTaskItem>> ListTasksAsync(int companyId, int userId, WorkflowTaskStatus? status = null, CancellationToken cancellationToken = default)
    {
        var result = new List<WorkflowTaskItem>();
        var connection = _dbContext.Database.GetDbConnection();
        var close = connection.State != ConnectionState.Open;
        if (close) await connection.OpenAsync(cancellationToken);
        try
        {
            await using var command = connection.CreateCommand();
            command.CommandText = @"SELECT Id,CompanyId,Module,DocumentType,DocumentReference,Title,Description,Amount,AssignedUserId,AssignedUserName,Status,Priority,CreatedAt,DueAt,RequestedByName,LastComment
FROM dbo.WorkflowTasks
WHERE CompanyId=@companyId AND AssignedUserId=@userId AND (@status IS NULL OR Status=@status)
ORDER BY CASE WHEN DueAt IS NOT NULL AND DueAt < SYSUTCDATETIME() AND Status=0 THEN 0 ELSE 1 END, Priority DESC, CreatedAt DESC;";
            Add(command, "@companyId", companyId); Add(command, "@userId", userId); Add(command, "@status", status.HasValue ? (int)status.Value : null);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken)) result.Add(Map(reader));
            return result;
        }
        finally { if (close) await connection.CloseAsync(); }
    }

    public async Task<WorkflowTaskItem?> FindTaskAsync(long taskId, int companyId, CancellationToken cancellationToken = default)
    {
        var connection = _dbContext.Database.GetDbConnection();
        var close = connection.State != ConnectionState.Open;
        if (close) await connection.OpenAsync(cancellationToken);
        try
        {
            await using var command = connection.CreateCommand();
            command.CommandText = @"SELECT Id,CompanyId,Module,DocumentType,DocumentReference,Title,Description,Amount,AssignedUserId,AssignedUserName,Status,Priority,CreatedAt,DueAt,RequestedByName,LastComment FROM dbo.WorkflowTasks WHERE Id=@id AND CompanyId=@companyId";
            Add(command, "@id", taskId); Add(command, "@companyId", companyId);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            return await reader.ReadAsync(cancellationToken) ? Map(reader) : null;
        }
        finally { if (close) await connection.CloseAsync(); }
    }

    public async Task DecideAsync(WorkflowTaskDecision decision, CancellationToken cancellationToken = default)
    {
        var connection = _dbContext.Database.GetDbConnection();
        var close = connection.State != ConnectionState.Open;
        if (close) await connection.OpenAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
        try
        {
            await using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = @"UPDATE dbo.WorkflowTasks SET Status=@status, LastComment=@comment, DecidedAt=SYSUTCDATETIME(), DecidedByUserId=@userId, DecidedByUserName=@userName WHERE Id=@id AND CompanyId=@companyId AND Status=0;
IF @@ROWCOUNT=0 THROW 50001, 'A tarefa já foi decidida ou não existe.', 1;
INSERT INTO dbo.WorkflowHistory (WorkflowTaskId, CompanyId, Action, UserId, UserName, Comment, CreatedAt)
VALUES (@id,@companyId,@action,@userId,@userName,@comment,SYSUTCDATETIME());";
            Add(command, "@status", (int)decision.Decision); Add(command, "@comment", decision.Comment);
            Add(command, "@userId", decision.UserId); Add(command, "@userName", decision.UserName);
            Add(command, "@id", decision.TaskId); Add(command, "@companyId", decision.CompanyId); Add(command, "@action", decision.Decision.ToString());
            await command.ExecuteNonQueryAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
        finally { if (close) await connection.CloseAsync(); }
    }

    public async Task<WorkflowSummary> GetSummaryAsync(int companyId, int userId, CancellationToken cancellationToken = default)
    {
        var connection = _dbContext.Database.GetDbConnection();
        var close = connection.State != ConnectionState.Open;
        if (close) await connection.OpenAsync(cancellationToken);
        try
        {
            await using var command = connection.CreateCommand();
            command.CommandText = @"SELECT
SUM(CASE WHEN Status=0 THEN 1 ELSE 0 END),
SUM(CASE WHEN Status=0 AND DueAt IS NOT NULL AND DueAt<SYSUTCDATETIME() THEN 1 ELSE 0 END),
SUM(CASE WHEN Status=0 AND Priority>=2 THEN 1 ELSE 0 END),
SUM(CASE WHEN Status IN (1,2,3) AND CONVERT(date,DecidedAt)=CONVERT(date,SYSUTCDATETIME()) THEN 1 ELSE 0 END)
FROM dbo.WorkflowTasks WHERE CompanyId=@companyId AND AssignedUserId=@userId;";
            Add(command, "@companyId", companyId); Add(command, "@userId", userId);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            if (!await reader.ReadAsync(cancellationToken)) return new WorkflowSummary(0,0,0,0);
            return new WorkflowSummary(GetInt(reader,0), GetInt(reader,1), GetInt(reader,2), GetInt(reader,3));
        }
        finally { if (close) await connection.CloseAsync(); }
    }

    private static int GetInt(DbDataReader reader, int ordinal) => reader.IsDBNull(ordinal) ? 0 : Convert.ToInt32(reader.GetValue(ordinal));
    private static void Add(DbCommand command, string name, object? value) { var p=command.CreateParameter(); p.ParameterName=name; p.Value=value ?? DBNull.Value; command.Parameters.Add(p); }
    private static WorkflowTaskItem Map(DbDataReader r) => new(
        r.GetInt64(0), r.GetInt32(1), r.GetString(2), r.GetString(3), r.GetString(4), r.GetString(5),
        r.IsDBNull(6)?null:r.GetString(6), r.IsDBNull(7)?null:r.GetDecimal(7), r.GetInt32(8), r.GetString(9),
        (WorkflowTaskStatus)r.GetInt32(10), (WorkflowPriority)r.GetInt32(11), r.GetDateTime(12), r.IsDBNull(13)?null:r.GetDateTime(13),
        r.IsDBNull(14)?null:r.GetString(14), r.IsDBNull(15)?null:r.GetString(15));
}

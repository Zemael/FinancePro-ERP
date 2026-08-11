using System.Data;
using System.Data.Common;
using FinancePro.Data.Context;
using FinancePro.Platform.Consolidation;
using Microsoft.EntityFrameworkCore;

namespace FinancePro.Data.Consolidation;

public sealed class SqlConsolidationStore : IConsolidationStore
{
    private readonly FinanceProDbContext _dbContext;
    public SqlConsolidationStore(FinanceProDbContext dbContext) => _dbContext = dbContext;

    public async Task<IReadOnlyList<ConsolidationCompany>> ListCompaniesAsync(CancellationToken cancellationToken = default)
    {
        var list = new List<ConsolidationCompany>();
        var cn = _dbContext.Database.GetDbConnection(); var close = cn.State != ConnectionState.Open; if (close) await cn.OpenAsync(cancellationToken);
        try
        {
            await using var cmd = cn.CreateCommand();
            cmd.CommandText = "SELECT Id,Nome FROM dbo.Empresas ORDER BY Nome,Id";
            await using var rd = await cmd.ExecuteReaderAsync(cancellationToken);
            while (await rd.ReadAsync(cancellationToken)) list.Add(new(rd.GetInt32(0), rd.GetString(1)));
            return list;
        }
        finally { if (close) await cn.CloseAsync(); }
    }

    public async Task<IReadOnlyList<ConsolidationGroup>> ListGroupsAsync(CancellationToken cancellationToken = default)
    {
        var result = new List<ConsolidationGroup>();
        var cn = _dbContext.Database.GetDbConnection(); var close = cn.State != ConnectionState.Open; if (close) await cn.OpenAsync(cancellationToken);
        try
        {
            var rows = new List<(int Id,string Name,int Year,DateTime Created,DateTime Updated)>();
            await using (var cmd = cn.CreateCommand())
            {
                cmd.CommandText = "SELECT Id,Name,FiscalYear,CreatedAt,UpdatedAt FROM dbo.ConsolidationGroups ORDER BY FiscalYear DESC,Name";
                await using var rd = await cmd.ExecuteReaderAsync(cancellationToken);
                while (await rd.ReadAsync(cancellationToken)) rows.Add((rd.GetInt32(0),rd.GetString(1),rd.GetInt32(2),rd.GetDateTime(3),rd.GetDateTime(4)));
            }
            foreach (var row in rows)
            {
                var ids = new List<int>();
                await using var cmd = cn.CreateCommand(); cmd.CommandText = "SELECT CompanyId FROM dbo.ConsolidationGroupCompanies WHERE GroupId=@id ORDER BY CompanyId"; Add(cmd,"@id",row.Id);
                await using var rd = await cmd.ExecuteReaderAsync(cancellationToken); while (await rd.ReadAsync(cancellationToken)) ids.Add(rd.GetInt32(0));
                result.Add(new(row.Id,row.Name,row.Year,ids,row.Created,row.Updated));
            }
            return result;
        }
        finally { if (close) await cn.CloseAsync(); }
    }

    public async Task<int> SaveGroupAsync(SaveConsolidationGroupRequest request, CancellationToken cancellationToken = default)
    {
        var cn = _dbContext.Database.GetDbConnection(); var close = cn.State != ConnectionState.Open; if (close) await cn.OpenAsync(cancellationToken); await using var tx = await cn.BeginTransactionAsync(cancellationToken);
        try
        {
            int id;
            if (request.Id is > 0)
            {
                id = request.Id.Value;
                await using var update = cn.CreateCommand(); update.Transaction = tx; update.CommandText = "UPDATE dbo.ConsolidationGroups SET Name=@name,FiscalYear=@year,UpdatedAt=SYSUTCDATETIME() WHERE Id=@id"; Add(update,"@name",request.Name);Add(update,"@year",request.FiscalYear);Add(update,"@id",id); await update.ExecuteNonQueryAsync(cancellationToken);
                await using var del = cn.CreateCommand(); del.Transaction=tx; del.CommandText="DELETE FROM dbo.ConsolidationGroupCompanies WHERE GroupId=@id"; Add(del,"@id",id); await del.ExecuteNonQueryAsync(cancellationToken);
            }
            else
            {
                await using var insert = cn.CreateCommand(); insert.Transaction = tx; insert.CommandText = "INSERT INTO dbo.ConsolidationGroups(Name,FiscalYear,CreatedAt,UpdatedAt) VALUES(@name,@year,SYSUTCDATETIME(),SYSUTCDATETIME()); SELECT CAST(SCOPE_IDENTITY() AS int);"; Add(insert,"@name",request.Name);Add(insert,"@year",request.FiscalYear); id = Convert.ToInt32(await insert.ExecuteScalarAsync(cancellationToken));
            }
            foreach (var companyId in request.CompanyIds.Distinct())
            {
                await using var member = cn.CreateCommand(); member.Transaction=tx; member.CommandText="INSERT INTO dbo.ConsolidationGroupCompanies(GroupId,CompanyId) VALUES(@groupId,@companyId)"; Add(member,"@groupId",id);Add(member,"@companyId",companyId); await member.ExecuteNonQueryAsync(cancellationToken);
            }
            await tx.CommitAsync(cancellationToken); return id;
        }
        catch { await tx.RollbackAsync(cancellationToken); throw; }
        finally { if (close) await cn.CloseAsync(); }
    }

    public async Task<IReadOnlyList<ConsolidationRun>> ListRunsAsync(int groupId, CancellationToken cancellationToken = default)
    {
        var list = new List<ConsolidationRun>(); var cn = _dbContext.Database.GetDbConnection(); var close=cn.State!=ConnectionState.Open;if(close)await cn.OpenAsync(cancellationToken);
        try
        {
            await using var cmd=cn.CreateCommand();cmd.CommandText=@"SELECT r.Id,r.GroupId,g.Name,r.FromDate,r.ToDate,r.Status,r.CreatedBy,r.CreatedByName,r.CreatedAt,r.ValidatedAt,r.ConsolidatedAt,r.ClosedAt
FROM dbo.ConsolidationRuns r INNER JOIN dbo.ConsolidationGroups g ON g.Id=r.GroupId WHERE r.GroupId=@groupId ORDER BY r.CreatedAt DESC,r.Id DESC";Add(cmd,"@groupId",groupId);
            await using var rd=await cmd.ExecuteReaderAsync(cancellationToken); while(await rd.ReadAsync(cancellationToken)) list.Add(ReadRun(rd)); return list;
        }
        finally{if(close)await cn.CloseAsync();}
    }

    public async Task<ConsolidationRun?> GetRunAsync(int runId, CancellationToken cancellationToken = default)
    {
        var cn=_dbContext.Database.GetDbConnection();var close=cn.State!=ConnectionState.Open;if(close)await cn.OpenAsync(cancellationToken);
        try{await using var cmd=cn.CreateCommand();cmd.CommandText=@"SELECT r.Id,r.GroupId,g.Name,r.FromDate,r.ToDate,r.Status,r.CreatedBy,r.CreatedByName,r.CreatedAt,r.ValidatedAt,r.ConsolidatedAt,r.ClosedAt
FROM dbo.ConsolidationRuns r INNER JOIN dbo.ConsolidationGroups g ON g.Id=r.GroupId WHERE r.Id=@id";Add(cmd,"@id",runId);await using var rd=await cmd.ExecuteReaderAsync(cancellationToken);return await rd.ReadAsync(cancellationToken)?ReadRun(rd):null;}finally{if(close)await cn.CloseAsync();}
    }

    public async Task<int> CreateRunAsync(CreateConsolidationRunRequest request, CancellationToken cancellationToken = default)
    {
        var cn=_dbContext.Database.GetDbConnection();var close=cn.State!=ConnectionState.Open;if(close)await cn.OpenAsync(cancellationToken);
        try{await using var cmd=cn.CreateCommand();cmd.CommandText=@"INSERT INTO dbo.ConsolidationRuns(GroupId,FromDate,ToDate,Status,CreatedBy,CreatedByName,CreatedAt) VALUES(@groupId,@from,@to,'Rascunho',@userId,@userName,SYSUTCDATETIME()); SELECT CAST(SCOPE_IDENTITY() AS int);";Add(cmd,"@groupId",request.GroupId);Add(cmd,"@from",request.FromDate);Add(cmd,"@to",request.ToDate);Add(cmd,"@userId",request.UserId);Add(cmd,"@userName",request.UserName);return Convert.ToInt32(await cmd.ExecuteScalarAsync(cancellationToken));}finally{if(close)await cn.CloseAsync();}
    }

    public async Task<IReadOnlyList<ConsolidatedTrialBalanceRow>> GetTrialBalanceAsync(int runId, CancellationToken cancellationToken = default)
    {
        var list=new List<ConsolidatedTrialBalanceRow>();var cn=_dbContext.Database.GetDbConnection();var close=cn.State!=ConnectionState.Open;if(close)await cn.OpenAsync(cancellationToken);
        try
        {
            await using var cmd=cn.CreateCommand();cmd.CommandText=@"
WITH RunData AS (SELECT GroupId,FromDate,ToDate FROM dbo.ConsolidationRuns WHERE Id=@runId),
Source AS (
 SELECT a.Code AccountCode,MAX(a.Name) AccountName,SUM(l.Debit) SourceDebit,SUM(l.Credit) SourceCredit
 FROM RunData r JOIN dbo.ConsolidationGroupCompanies gc ON gc.GroupId=r.GroupId
 JOIN dbo.AccountingEntries e ON e.CompanyId=gc.CompanyId AND e.Status='Contabilizado' AND e.EntryDate>=r.FromDate AND e.EntryDate<DATEADD(day,1,r.ToDate)
 JOIN dbo.AccountingEntryLines l ON l.AccountingEntryId=e.Id JOIN dbo.EnterpriseChartAccounts a ON a.Id=l.AccountId
 GROUP BY a.Code),
Elim AS (SELECT AccountCode,SUM(Debit) EliminationDebit,SUM(Credit) EliminationCredit FROM dbo.ConsolidationEliminations WHERE RunId=@runId GROUP BY AccountCode),
Names AS (SELECT Code,MAX(Name) AccountName FROM dbo.EnterpriseChartAccounts GROUP BY Code)
SELECT COALESCE(s.AccountCode,e.AccountCode),COALESCE(s.AccountName,n.AccountName,COALESCE(s.AccountCode,e.AccountCode)),
COALESCE(s.SourceDebit,0),COALESCE(s.SourceCredit,0),COALESCE(e.EliminationDebit,0),COALESCE(e.EliminationCredit,0),
COALESCE(s.SourceDebit,0)+COALESCE(e.EliminationDebit,0),COALESCE(s.SourceCredit,0)+COALESCE(e.EliminationCredit,0)
FROM Source s FULL OUTER JOIN Elim e ON e.AccountCode=s.AccountCode LEFT JOIN Names n ON n.Code=COALESCE(s.AccountCode,e.AccountCode)
ORDER BY COALESCE(s.AccountCode,e.AccountCode)";Add(cmd,"@runId",runId);
            await using var rd=await cmd.ExecuteReaderAsync(cancellationToken);while(await rd.ReadAsync(cancellationToken))list.Add(new(rd.GetString(0),rd.GetString(1),rd.GetDecimal(2),rd.GetDecimal(3),rd.GetDecimal(4),rd.GetDecimal(5),rd.GetDecimal(6),rd.GetDecimal(7)));return list;
        }
        finally{if(close)await cn.CloseAsync();}
    }

    public async Task<IReadOnlyList<IntercompanyDifference>> GetIntercompanyDifferencesAsync(int runId, CancellationToken cancellationToken = default)
    {
        var list=new List<IntercompanyDifference>();var cn=_dbContext.Database.GetDbConnection();var close=cn.State!=ConnectionState.Open;if(close)await cn.OpenAsync(cancellationToken);
        try
        {
            await using var cmd=cn.CreateCommand();cmd.CommandText=@"
WITH RunData AS (SELECT GroupId,FromDate,ToDate FROM dbo.ConsolidationRuns WHERE Id=@runId),
CompanyAmounts AS (
 SELECT e.Reference,e.CompanyId,MAX(c.Nome) CompanyName,SUM(l.Debit) Amount
 FROM RunData r JOIN dbo.ConsolidationGroupCompanies gc ON gc.GroupId=r.GroupId
 JOIN dbo.AccountingEntries e ON e.CompanyId=gc.CompanyId AND e.Status='Contabilizado' AND e.EntryDate>=r.FromDate AND e.EntryDate<DATEADD(day,1,r.ToDate)
 JOIN dbo.AccountingEntryLines l ON l.AccountingEntryId=e.Id JOIN dbo.Empresas c ON c.Id=e.CompanyId
 WHERE e.Reference LIKE 'IC-%' OR e.Reference LIKE 'INTERCO-%'
 GROUP BY e.Reference,e.CompanyId)
SELECT Reference,COUNT(*) CompanyCount,STRING_AGG(CONVERT(nvarchar(max),CompanyName),', ') Companies,MIN(Amount),MAX(Amount),MAX(Amount)-MIN(Amount)
FROM CompanyAmounts GROUP BY Reference ORDER BY Reference";Add(cmd,"@runId",runId);
            await using var rd=await cmd.ExecuteReaderAsync(cancellationToken);while(await rd.ReadAsync(cancellationToken))list.Add(new(rd.GetString(0),rd.GetInt32(1),rd.GetString(2),rd.GetDecimal(3),rd.GetDecimal(4),rd.GetDecimal(5)));return list;
        }
        finally{if(close)await cn.CloseAsync();}
    }

    public async Task<IReadOnlyList<ConsolidationElimination>> ListEliminationsAsync(int runId, CancellationToken cancellationToken = default)
    {
        var list=new List<ConsolidationElimination>();var cn=_dbContext.Database.GetDbConnection();var close=cn.State!=ConnectionState.Open;if(close)await cn.OpenAsync(cancellationToken);
        try{await using var cmd=cn.CreateCommand();cmd.CommandText="SELECT Id,RunId,AccountCode,Description,Debit,Credit,Reference,CreatedBy,CreatedByName,CreatedAt FROM dbo.ConsolidationEliminations WHERE RunId=@runId ORDER BY Id";Add(cmd,"@runId",runId);await using var rd=await cmd.ExecuteReaderAsync(cancellationToken);while(await rd.ReadAsync(cancellationToken))list.Add(new(rd.GetInt32(0),rd.GetInt32(1),rd.GetString(2),rd.GetString(3),rd.GetDecimal(4),rd.GetDecimal(5),rd.GetString(6),rd.GetInt32(7),rd.GetString(8),rd.GetDateTime(9)));return list;}finally{if(close)await cn.CloseAsync();}
    }

    public async Task<int> SaveEliminationAsync(SaveConsolidationEliminationRequest request, CancellationToken cancellationToken = default)
    {
        var cn=_dbContext.Database.GetDbConnection();var close=cn.State!=ConnectionState.Open;if(close)await cn.OpenAsync(cancellationToken);
        try{await using var cmd=cn.CreateCommand();cmd.CommandText=@"INSERT INTO dbo.ConsolidationEliminations(RunId,AccountCode,Description,Debit,Credit,Reference,CreatedBy,CreatedByName,CreatedAt) VALUES(@runId,@account,@description,@debit,@credit,@reference,@userId,@userName,SYSUTCDATETIME()); SELECT CAST(SCOPE_IDENTITY() AS int);";Add(cmd,"@runId",request.RunId);Add(cmd,"@account",request.AccountCode);Add(cmd,"@description",request.Description);Add(cmd,"@debit",request.Debit);Add(cmd,"@credit",request.Credit);Add(cmd,"@reference",request.Reference);Add(cmd,"@userId",request.UserId);Add(cmd,"@userName",request.UserName);return Convert.ToInt32(await cmd.ExecuteScalarAsync(cancellationToken));}finally{if(close)await cn.CloseAsync();}
    }

    public async Task DeleteEliminationAsync(int runId, int eliminationId, CancellationToken cancellationToken = default)
    {
        var cn=_dbContext.Database.GetDbConnection();var close=cn.State!=ConnectionState.Open;if(close)await cn.OpenAsync(cancellationToken);try{await using var cmd=cn.CreateCommand();cmd.CommandText="DELETE FROM dbo.ConsolidationEliminations WHERE Id=@id AND RunId=@runId";Add(cmd,"@id",eliminationId);Add(cmd,"@runId",runId);await cmd.ExecuteNonQueryAsync(cancellationToken);}finally{if(close)await cn.CloseAsync();}
    }

    public async Task SetRunStatusAsync(int runId, string status, int userId, string userName, CancellationToken cancellationToken = default)
    {
        var cn=_dbContext.Database.GetDbConnection();var close=cn.State!=ConnectionState.Open;if(close)await cn.OpenAsync(cancellationToken);await using var tx=await cn.BeginTransactionAsync(cancellationToken);
        try
        {
            string old;
            await using(var q=cn.CreateCommand()){q.Transaction=tx;q.CommandText="SELECT Status FROM dbo.ConsolidationRuns WITH (UPDLOCK,ROWLOCK) WHERE Id=@id";Add(q,"@id",runId);old=Convert.ToString(await q.ExecuteScalarAsync(cancellationToken))??throw new KeyNotFoundException("Processamento não encontrado.");}
            await using(var u=cn.CreateCommand()){u.Transaction=tx;u.CommandText=@"UPDATE dbo.ConsolidationRuns SET Status=@status,
ValidatedAt=CASE WHEN @status='Validado' THEN COALESCE(ValidatedAt,SYSUTCDATETIME()) ELSE ValidatedAt END,
ConsolidatedAt=CASE WHEN @status='Consolidado' THEN COALESCE(ConsolidatedAt,SYSUTCDATETIME()) ELSE ConsolidatedAt END,
ClosedAt=CASE WHEN @status='Fechado' THEN COALESCE(ClosedAt,SYSUTCDATETIME()) ELSE ClosedAt END WHERE Id=@id";Add(u,"@status",status);Add(u,"@id",runId);await u.ExecuteNonQueryAsync(cancellationToken);}
            await using(var h=cn.CreateCommand()){h.Transaction=tx;h.CommandText="INSERT INTO dbo.ConsolidationRunHistory(RunId,PreviousStatus,NewStatus,UserId,UserName,CreatedAt) VALUES(@id,@old,@new,@userId,@userName,SYSUTCDATETIME())";Add(h,"@id",runId);Add(h,"@old",old);Add(h,"@new",status);Add(h,"@userId",userId);Add(h,"@userName",userName);await h.ExecuteNonQueryAsync(cancellationToken);}
            await tx.CommitAsync(cancellationToken);
        }
        catch{await tx.RollbackAsync(cancellationToken);throw;}finally{if(close)await cn.CloseAsync();}
    }

    private static ConsolidationRun ReadRun(DbDataReader rd) => new(rd.GetInt32(0),rd.GetInt32(1),rd.GetString(2),rd.GetDateTime(3),rd.GetDateTime(4),rd.GetString(5),rd.GetInt32(6),rd.GetString(7),rd.GetDateTime(8),rd.IsDBNull(9)?null:rd.GetDateTime(9),rd.IsDBNull(10)?null:rd.GetDateTime(10),rd.IsDBNull(11)?null:rd.GetDateTime(11));
    private static void Add(DbCommand cmd,string name,object? value){var p=cmd.CreateParameter();p.ParameterName=name;p.Value=value??DBNull.Value;cmd.Parameters.Add(p);}
}

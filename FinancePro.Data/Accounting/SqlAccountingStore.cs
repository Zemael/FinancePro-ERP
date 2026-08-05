using System.Data;
using System.Data.Common;
using FinancePro.Data.Context;
using FinancePro.Platform.Accounting;
using Microsoft.EntityFrameworkCore;

namespace FinancePro.Data.Accounting;

public sealed class SqlAccountingStore : IAccountingStore
{
    private readonly FinanceProDbContext _dbContext;
    public SqlAccountingStore(FinanceProDbContext dbContext) => _dbContext = dbContext;

    public async Task<IReadOnlyList<AccountingEntry>> ListAsync(int companyId, DateTime? from = null, DateTime? to = null, CancellationToken cancellationToken = default)
    {
        var headers = new List<(int Id, DateTime Date, string Number, string Reference, string Description, string Module, string Status, int UserId, string UserName, DateTime CreatedAt)>();
        var cn = _dbContext.Database.GetDbConnection(); var close = cn.State != ConnectionState.Open;
        if (close) await cn.OpenAsync(cancellationToken);
        try
        {
            await using var cmd = cn.CreateCommand();
            cmd.CommandText = @"SELECT Id,EntryDate,DocumentNumber,Reference,Description,SourceModule,Status,CreatedBy,CreatedByName,CreatedAt
FROM dbo.AccountingEntries WHERE CompanyId=@companyId
AND (@from IS NULL OR EntryDate>=@from) AND (@to IS NULL OR EntryDate<DATEADD(day,1,@to))
ORDER BY EntryDate DESC,Id DESC";
            Add(cmd, "@companyId", companyId); Add(cmd, "@from", from); Add(cmd, "@to", to);
            await using var rd = await cmd.ExecuteReaderAsync(cancellationToken);
            while (await rd.ReadAsync(cancellationToken))
                headers.Add((rd.GetInt32(0),rd.GetDateTime(1),rd.GetString(2),rd.GetString(3),rd.GetString(4),rd.GetString(5),rd.GetString(6),rd.GetInt32(7),rd.GetString(8),rd.GetDateTime(9)));

            var result = new List<AccountingEntry>();
            foreach (var h in headers)
                result.Add(new AccountingEntry(h.Id, companyId, h.Date, h.Number, h.Reference, h.Description, h.Module, h.Status, h.UserId, h.UserName, h.CreatedAt, await ListLinesAsync(cn, h.Id, cancellationToken)));
            return result;
        }
        finally { if (close) await cn.CloseAsync(); }
    }

    public async Task<AccountingEntry?> GetAsync(int companyId, int id, CancellationToken cancellationToken = default)
        => (await ListAsync(companyId, null, null, cancellationToken)).FirstOrDefault(x => x.Id == id);

    public async Task<int> SaveAsync(SaveAccountingEntryRequest request, CancellationToken cancellationToken = default)
    {
        var cn = _dbContext.Database.GetDbConnection(); var close = cn.State != ConnectionState.Open;
        if (close) await cn.OpenAsync(cancellationToken);
        await using var tx = await cn.BeginTransactionAsync(cancellationToken);
        try
        {
            int id;
            await using (var cmd = cn.CreateCommand())
            {
                cmd.Transaction = tx;
                cmd.CommandText = request.Id is null
                    ? @"INSERT INTO dbo.AccountingEntries(CompanyId,EntryDate,DocumentNumber,Reference,Description,SourceModule,Status,CreatedBy,CreatedByName,CreatedAt,UpdatedAt)
VALUES(@companyId,@date,@number,@reference,@description,@module,'Rascunho',@userId,@userName,SYSUTCDATETIME(),SYSUTCDATETIME()); SELECT CAST(SCOPE_IDENTITY() AS int);"
                    : @"UPDATE dbo.AccountingEntries SET EntryDate=@date,DocumentNumber=@number,Reference=@reference,Description=@description,SourceModule=@module,UpdatedAt=SYSUTCDATETIME()
WHERE Id=@id AND CompanyId=@companyId AND Status='Rascunho'; SELECT @id;";
                AddCommon(cmd, request); Add(cmd, "@id", request.Id);
                id = Convert.ToInt32(await cmd.ExecuteScalarAsync(cancellationToken));
            }
            await using (var del = cn.CreateCommand()) { del.Transaction=tx; del.CommandText="DELETE FROM dbo.AccountingEntryLines WHERE AccountingEntryId=@id"; Add(del,"@id",id); await del.ExecuteNonQueryAsync(cancellationToken); }
            foreach (var line in request.Lines)
            {
                await using var cmd = cn.CreateCommand(); cmd.Transaction = tx;
                cmd.CommandText=@"INSERT INTO dbo.AccountingEntryLines(AccountingEntryId,AccountId,CostCenterId,Description,Debit,Credit)
VALUES(@entryId,@accountId,@costCenterId,@description,@debit,@credit)";
                Add(cmd,"@entryId",id);Add(cmd,"@accountId",line.AccountId);Add(cmd,"@costCenterId",line.CostCenterId);Add(cmd,"@description",line.Description);Add(cmd,"@debit",line.Debit);Add(cmd,"@credit",line.Credit);
                await cmd.ExecuteNonQueryAsync(cancellationToken);
            }
            await tx.CommitAsync(cancellationToken); return id;
        }
        catch { await tx.RollbackAsync(cancellationToken); throw; }
        finally { if (close) await cn.CloseAsync(); }
    }

    public async Task SetStatusAsync(int companyId, int id, string status, int userId, string userName, CancellationToken cancellationToken = default)
    {
        var cn = _dbContext.Database.GetDbConnection(); var close = cn.State != ConnectionState.Open;
        if (close) await cn.OpenAsync(cancellationToken);
        try
        {
            await using var cmd = cn.CreateCommand();
            cmd.CommandText=@"UPDATE dbo.AccountingEntries SET Status=@status,PostedBy=CASE WHEN @status='Contabilizado' THEN @userId ELSE PostedBy END,
PostedByName=CASE WHEN @status='Contabilizado' THEN @userName ELSE PostedByName END,PostedAt=CASE WHEN @status='Contabilizado' THEN SYSUTCDATETIME() ELSE PostedAt END,
UpdatedAt=SYSUTCDATETIME() WHERE Id=@id AND CompanyId=@companyId";
            Add(cmd,"@companyId",companyId);Add(cmd,"@id",id);Add(cmd,"@status",status);Add(cmd,"@userId",userId);Add(cmd,"@userName",userName);
            await cmd.ExecuteNonQueryAsync(cancellationToken);
        }
        finally { if (close) await cn.CloseAsync(); }
    }


    public async Task<IReadOnlyList<GeneralJournalRow>> GetGeneralJournalAsync(int companyId, DateTime from, DateTime to, string? status = null, string? search = null, CancellationToken cancellationToken = default)
    {
        var list = new List<GeneralJournalRow>();
        var cn = _dbContext.Database.GetDbConnection(); var close = cn.State != ConnectionState.Open;
        if (close) await cn.OpenAsync(cancellationToken);
        try
        {
            await using var cmd = cn.CreateCommand();
            cmd.CommandText = @"SELECT e.Id,e.EntryDate,e.DocumentNumber,e.Reference,e.Description,e.SourceModule,e.Status,
 a.Code,a.Name,l.Description,l.Debit,l.Credit
FROM dbo.AccountingEntries e
INNER JOIN dbo.AccountingEntryLines l ON l.AccountingEntryId=e.Id
INNER JOIN dbo.EnterpriseChartAccounts a ON a.Id=l.AccountId
WHERE e.CompanyId=@companyId AND e.EntryDate>=@from AND e.EntryDate<DATEADD(day,1,@to)
AND (@status IS NULL OR @status='' OR e.Status=@status)
AND (@search IS NULL OR @search='' OR e.DocumentNumber LIKE '%'+@search+'%' OR e.Reference LIKE '%'+@search+'%' OR e.Description LIKE '%'+@search+'%')
ORDER BY e.EntryDate,e.Id,l.Id";
            Add(cmd,"@companyId",companyId); Add(cmd,"@from",from); Add(cmd,"@to",to); Add(cmd,"@status",status); Add(cmd,"@search",search);
            await using var rd = await cmd.ExecuteReaderAsync(cancellationToken);
            while(await rd.ReadAsync(cancellationToken))
                list.Add(new GeneralJournalRow(rd.GetInt32(0),rd.GetDateTime(1),rd.GetString(2),rd.GetString(3),rd.GetString(4),rd.GetString(5),rd.GetString(6),rd.GetString(7),rd.GetString(8),rd.GetString(9),rd.GetDecimal(10),rd.GetDecimal(11)));
            return list;
        }
        finally { if (close) await cn.CloseAsync(); }
    }

    public async Task<IReadOnlyList<GeneralLedgerRow>> GetGeneralLedgerAsync(int companyId, int accountId, DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        var list = new List<GeneralLedgerRow>(); decimal running = 0;
        var cn = _dbContext.Database.GetDbConnection(); var close = cn.State != ConnectionState.Open;
        if (close) await cn.OpenAsync(cancellationToken);
        try
        {
            await using (var opening = cn.CreateCommand())
            {
                opening.CommandText=@"SELECT COALESCE(SUM(l.Debit-l.Credit),0) FROM dbo.AccountingEntryLines l INNER JOIN dbo.AccountingEntries e ON e.Id=l.AccountingEntryId WHERE e.CompanyId=@companyId AND l.AccountId=@accountId AND e.Status='Contabilizado' AND e.EntryDate<@from";
                Add(opening,"@companyId",companyId);Add(opening,"@accountId",accountId);Add(opening,"@from",from);
                running=Convert.ToDecimal(await opening.ExecuteScalarAsync(cancellationToken));
            }
            await using var cmd=cn.CreateCommand();
            cmd.CommandText=@"SELECT e.EntryDate,e.DocumentNumber,e.Reference,COALESCE(NULLIF(l.Description,''),e.Description),l.Debit,l.Credit
FROM dbo.AccountingEntryLines l INNER JOIN dbo.AccountingEntries e ON e.Id=l.AccountingEntryId
WHERE e.CompanyId=@companyId AND l.AccountId=@accountId AND e.Status='Contabilizado' AND e.EntryDate>=@from AND e.EntryDate<DATEADD(day,1,@to)
ORDER BY e.EntryDate,e.Id,l.Id";
            Add(cmd,"@companyId",companyId);Add(cmd,"@accountId",accountId);Add(cmd,"@from",from);Add(cmd,"@to",to);
            await using var rd=await cmd.ExecuteReaderAsync(cancellationToken);
            while(await rd.ReadAsync(cancellationToken)) { var d=rd.GetDecimal(4);var c=rd.GetDecimal(5);running+=d-c;list.Add(new GeneralLedgerRow(rd.GetDateTime(0),rd.GetString(1),rd.GetString(2),rd.GetString(3),d,c,running)); }
            return list;
        }
        finally { if (close) await cn.CloseAsync(); }
    }

    public async Task<IReadOnlyList<TrialBalanceRow>> GetTrialBalanceAsync(int companyId, DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        var list=new List<TrialBalanceRow>();
        var cn=_dbContext.Database.GetDbConnection();var close=cn.State!=ConnectionState.Open;if(close)await cn.OpenAsync(cancellationToken);
        try
        {
            await using var cmd=cn.CreateCommand();
            cmd.CommandText=@"WITH M AS (
 SELECT a.Id AccountId,a.Code,a.Name,
 SUM(CASE WHEN e.EntryDate<@from THEN l.Debit ELSE 0 END) OpeningDebitRaw,
 SUM(CASE WHEN e.EntryDate<@from THEN l.Credit ELSE 0 END) OpeningCreditRaw,
 SUM(CASE WHEN e.EntryDate>=@from AND e.EntryDate<DATEADD(day,1,@to) THEN l.Debit ELSE 0 END) PeriodDebit,
 SUM(CASE WHEN e.EntryDate>=@from AND e.EntryDate<DATEADD(day,1,@to) THEN l.Credit ELSE 0 END) PeriodCredit
 FROM dbo.EnterpriseChartAccounts a
 LEFT JOIN dbo.AccountingEntryLines l ON l.AccountId=a.Id
 LEFT JOIN dbo.AccountingEntries e ON e.Id=l.AccountingEntryId AND e.Status='Contabilizado' AND e.CompanyId=@companyId AND e.EntryDate<DATEADD(day,1,@to)
 WHERE a.CompanyId=@companyId
 GROUP BY a.Id,a.Code,a.Name)
SELECT AccountId,Code,Name,
 CASE WHEN OpeningDebitRaw-OpeningCreditRaw>0 THEN OpeningDebitRaw-OpeningCreditRaw ELSE 0 END,
 CASE WHEN OpeningCreditRaw-OpeningDebitRaw>0 THEN OpeningCreditRaw-OpeningDebitRaw ELSE 0 END,
 PeriodDebit,PeriodCredit,
 CASE WHEN (OpeningDebitRaw-OpeningCreditRaw)+(PeriodDebit-PeriodCredit)>0 THEN (OpeningDebitRaw-OpeningCreditRaw)+(PeriodDebit-PeriodCredit) ELSE 0 END,
 CASE WHEN (OpeningCreditRaw-OpeningDebitRaw)+(PeriodCredit-PeriodDebit)>0 THEN (OpeningCreditRaw-OpeningDebitRaw)+(PeriodCredit-PeriodDebit) ELSE 0 END
FROM M WHERE OpeningDebitRaw<>0 OR OpeningCreditRaw<>0 OR PeriodDebit<>0 OR PeriodCredit<>0 ORDER BY Code";
            Add(cmd,"@companyId",companyId);Add(cmd,"@from",from);Add(cmd,"@to",to);
            await using var rd=await cmd.ExecuteReaderAsync(cancellationToken);
            while(await rd.ReadAsync(cancellationToken)) list.Add(new TrialBalanceRow(rd.GetInt32(0),rd.GetString(1),rd.GetString(2),rd.GetDecimal(3),rd.GetDecimal(4),rd.GetDecimal(5),rd.GetDecimal(6),rd.GetDecimal(7),rd.GetDecimal(8)));
            return list;
        }
        finally { if(close) await cn.CloseAsync(); }
    }


    public async Task<IReadOnlyList<IncomeStatementRow>> GetIncomeStatementAsync(int companyId, DateTime from, DateTime to, DateTime previousFrom, DateTime previousTo, CancellationToken cancellationToken = default)
    {
        var list = new List<IncomeStatementRow>();
        var cn = _dbContext.Database.GetDbConnection(); var close = cn.State != ConnectionState.Open;
        if (close) await cn.OpenAsync(cancellationToken);
        try
        {
            await using var cmd = cn.CreateCommand();
            cmd.CommandText = @"SELECT a.Code,a.Name,
CASE
 WHEN a.Code LIKE '41%' THEN 'Deducao'
 WHEN a.Code LIKE '4%' THEN 'Receita'
 WHEN a.Code LIKE '5%' THEN 'Custo'
 WHEN a.Code LIKE '6%' THEN 'DespesaOperacional'
 WHEN a.Code LIKE '7%' THEN 'ResultadoFinanceiro'
 WHEN a.Code LIKE '8%' THEN 'Imposto'
 ELSE '' END LineType,
SUM(CASE WHEN e.EntryDate>=@from AND e.EntryDate<DATEADD(day,1,@to) THEN
 CASE WHEN a.Code LIKE '4%' OR a.Code LIKE '7%' THEN l.Credit-l.Debit ELSE l.Debit-l.Credit END ELSE 0 END) CurrentAmount,
SUM(CASE WHEN e.EntryDate>=@previousFrom AND e.EntryDate<DATEADD(day,1,@previousTo) THEN
 CASE WHEN a.Code LIKE '4%' OR a.Code LIKE '7%' THEN l.Credit-l.Debit ELSE l.Debit-l.Credit END ELSE 0 END) PreviousAmount
FROM dbo.EnterpriseChartAccounts a
INNER JOIN dbo.AccountingEntryLines l ON l.AccountId=a.Id
INNER JOIN dbo.AccountingEntries e ON e.Id=l.AccountingEntryId
WHERE a.CompanyId=@companyId AND e.CompanyId=@companyId AND e.Status='Contabilizado'
AND (a.Code LIKE '4%' OR a.Code LIKE '5%' OR a.Code LIKE '6%' OR a.Code LIKE '7%' OR a.Code LIKE '8%')
AND ((e.EntryDate>=@from AND e.EntryDate<DATEADD(day,1,@to)) OR (e.EntryDate>=@previousFrom AND e.EntryDate<DATEADD(day,1,@previousTo)))
GROUP BY a.Code,a.Name
HAVING SUM(CASE WHEN e.EntryDate>=@from AND e.EntryDate<DATEADD(day,1,@to) THEN l.Debit+l.Credit ELSE 0 END)<>0
 OR SUM(CASE WHEN e.EntryDate>=@previousFrom AND e.EntryDate<DATEADD(day,1,@previousTo) THEN l.Debit+l.Credit ELSE 0 END)<>0
ORDER BY a.Code";
            Add(cmd,"@companyId",companyId); Add(cmd,"@from",from); Add(cmd,"@to",to); Add(cmd,"@previousFrom",previousFrom); Add(cmd,"@previousTo",previousTo);
            await using var rd = await cmd.ExecuteReaderAsync(cancellationToken);
            while (await rd.ReadAsync(cancellationToken))
                list.Add(new IncomeStatementRow(rd.GetString(0), rd.GetString(1), rd.GetString(2), rd.GetDecimal(3), rd.GetDecimal(4)));
            return list;
        }
        finally { if (close) await cn.CloseAsync(); }
    }

    public async Task<IReadOnlyList<BalanceSheetRow>> GetBalanceSheetAsync(int companyId, DateTime asOf, DateTime previousAsOf, CancellationToken cancellationToken = default)
    {
        var list = new List<BalanceSheetRow>();
        var cn = _dbContext.Database.GetDbConnection(); var close = cn.State != ConnectionState.Open;
        if (close) await cn.OpenAsync(cancellationToken);
        try
        {
            await using var cmd = cn.CreateCommand();
            cmd.CommandText = @"SELECT a.Code,a.Name,
CASE WHEN a.Code LIKE '10%' OR a.Code LIKE '11%' OR a.Code LIKE '12%' THEN 'AtivoCirculante'
     WHEN a.Code LIKE '1%' THEN 'AtivoNaoCirculante'
     WHEN a.Code LIKE '20%' OR a.Code LIKE '21%' OR a.Code LIKE '22%' THEN 'PassivoCirculante'
     WHEN a.Code LIKE '2%' THEN 'PassivoNaoCirculante'
     WHEN a.Code LIKE '3%' THEN 'PatrimonioLiquido' ELSE '' END Section,
SUM(CASE WHEN e.EntryDate<DATEADD(day,1,@asOf) THEN CASE WHEN a.Code LIKE '1%' THEN l.Debit-l.Credit ELSE l.Credit-l.Debit END ELSE 0 END) CurrentAmount,
SUM(CASE WHEN e.EntryDate<DATEADD(day,1,@previousAsOf) THEN CASE WHEN a.Code LIKE '1%' THEN l.Debit-l.Credit ELSE l.Credit-l.Debit END ELSE 0 END) PreviousAmount
FROM dbo.EnterpriseChartAccounts a
INNER JOIN dbo.AccountingEntryLines l ON l.AccountId=a.Id
INNER JOIN dbo.AccountingEntries e ON e.Id=l.AccountingEntryId
WHERE a.CompanyId=@companyId AND e.CompanyId=@companyId AND e.Status='Contabilizado'
AND (a.Code LIKE '1%' OR a.Code LIKE '2%' OR a.Code LIKE '3%')
GROUP BY a.Code,a.Name
HAVING SUM(CASE WHEN e.EntryDate<DATEADD(day,1,@asOf) THEN l.Debit+l.Credit ELSE 0 END)<>0
 OR SUM(CASE WHEN e.EntryDate<DATEADD(day,1,@previousAsOf) THEN l.Debit+l.Credit ELSE 0 END)<>0
ORDER BY a.Code";
            Add(cmd,"@companyId",companyId); Add(cmd,"@asOf",asOf); Add(cmd,"@previousAsOf",previousAsOf);
            await using var rd = await cmd.ExecuteReaderAsync(cancellationToken);
            while (await rd.ReadAsync(cancellationToken))
                list.Add(new BalanceSheetRow(rd.GetString(0),rd.GetString(1),rd.GetString(2),rd.GetDecimal(3),rd.GetDecimal(4)));
            return list;
        }
        finally { if (close) await cn.CloseAsync(); }
    }

    public async Task<IReadOnlyList<CashFlowRow>> GetDirectCashFlowAsync(int companyId, DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        var list = new List<CashFlowRow>();
        var cn = _dbContext.Database.GetDbConnection(); var close = cn.State != ConnectionState.Open;
        if (close) await cn.OpenAsync(cancellationToken);
        try
        {
            await using var cmd = cn.CreateCommand();
            cmd.CommandText = @"SELECT e.EntryDate,e.DocumentNumber,e.Description,
CASE WHEN e.SourceModule IN ('Assets','Patrimonio','Património') THEN 'Investimento'
     WHEN e.SourceModule IN ('Financing','Financiamento','Capital','Loans','Emprestimos','Empréstimos') THEN 'Financiamento'
     ELSE 'Operacional' END Activity,
SUM(CASE WHEN l.Debit-l.Credit>0 THEN l.Debit-l.Credit ELSE 0 END) Inflow,
SUM(CASE WHEN l.Debit-l.Credit<0 THEN l.Credit-l.Debit ELSE 0 END) Outflow
FROM dbo.AccountingEntries e
INNER JOIN dbo.AccountingEntryLines l ON l.AccountingEntryId=e.Id
INNER JOIN dbo.EnterpriseChartAccounts a ON a.Id=l.AccountId
WHERE e.CompanyId=@companyId AND e.Status='Contabilizado'
AND e.EntryDate>=@from AND e.EntryDate<DATEADD(day,1,@to)
AND (a.Code LIKE '10%' OR a.Code LIKE '11%' OR a.Code LIKE '12%')
GROUP BY e.EntryDate,e.DocumentNumber,e.Description,e.SourceModule,e.Id
HAVING SUM(l.Debit-l.Credit)<>0
ORDER BY e.EntryDate,e.Id";
            Add(cmd,"@companyId",companyId); Add(cmd,"@from",from); Add(cmd,"@to",to);
            await using var rd = await cmd.ExecuteReaderAsync(cancellationToken);
            while (await rd.ReadAsync(cancellationToken))
                list.Add(new CashFlowRow(rd.GetDateTime(0),rd.GetString(1),rd.GetString(2),rd.GetString(3),rd.GetDecimal(4),rd.GetDecimal(5)));
            return list;
        }
        finally { if (close) await cn.CloseAsync(); }
    }

    public async Task<decimal> GetCashBalanceAsync(int companyId, DateTime asOf, CancellationToken cancellationToken = default)
    {
        var cn = _dbContext.Database.GetDbConnection(); var close = cn.State != ConnectionState.Open;
        if (close) await cn.OpenAsync(cancellationToken);
        try
        {
            await using var cmd = cn.CreateCommand();
            cmd.CommandText = @"SELECT COALESCE(SUM(l.Debit-l.Credit),0)
FROM dbo.AccountingEntryLines l
INNER JOIN dbo.AccountingEntries e ON e.Id=l.AccountingEntryId
INNER JOIN dbo.EnterpriseChartAccounts a ON a.Id=l.AccountId
WHERE e.CompanyId=@companyId AND e.Status='Contabilizado' AND e.EntryDate<DATEADD(day,1,@asOf)
AND (a.Code LIKE '10%' OR a.Code LIKE '11%' OR a.Code LIKE '12%')";
            Add(cmd,"@companyId",companyId); Add(cmd,"@asOf",asOf);
            return Convert.ToDecimal(await cmd.ExecuteScalarAsync(cancellationToken));
        }
        finally { if (close) await cn.CloseAsync(); }
    }

    private static async Task<IReadOnlyList<AccountingEntryLine>> ListLinesAsync(DbConnection cn, int entryId, CancellationToken ct)
    {
        var list=new List<AccountingEntryLine>(); await using var cmd=cn.CreateCommand();
        cmd.CommandText=@"SELECT l.Id,l.AccountId,a.Code,a.Name,l.CostCenterId,c.Code,l.Description,l.Debit,l.Credit
FROM dbo.AccountingEntryLines l INNER JOIN dbo.EnterpriseChartAccounts a ON a.Id=l.AccountId
LEFT JOIN dbo.CostCenters c ON c.Id=l.CostCenterId WHERE l.AccountingEntryId=@id ORDER BY l.Id";
        Add(cmd,"@id",entryId); await using var rd=await cmd.ExecuteReaderAsync(ct);
        while(await rd.ReadAsync(ct)) list.Add(new AccountingEntryLine(rd.GetInt32(0),rd.GetInt32(1),rd.GetString(2),rd.GetString(3),rd.IsDBNull(4)?null:rd.GetInt32(4),rd.IsDBNull(5)?null:rd.GetString(5),rd.GetString(6),rd.GetDecimal(7),rd.GetDecimal(8)));
        return list;
    }

    private static void AddCommon(DbCommand cmd, SaveAccountingEntryRequest x){Add(cmd,"@companyId",x.CompanyId);Add(cmd,"@date",x.EntryDate);Add(cmd,"@number",x.DocumentNumber);Add(cmd,"@reference",x.Reference);Add(cmd,"@description",x.Description);Add(cmd,"@module",x.SourceModule);Add(cmd,"@userId",x.UserId);Add(cmd,"@userName",x.UserName);}
    private static void Add(DbCommand cmd,string name,object? value){var p=cmd.CreateParameter();p.ParameterName=name;p.Value=value??DBNull.Value;cmd.Parameters.Add(p);}
}

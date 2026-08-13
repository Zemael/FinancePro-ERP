using System.Data;
using FinancePro.Data.Context;
using FinancePro.Platform.Accounting;
using Microsoft.EntityFrameworkCore;

namespace FinancePro.Data.Accounting;

public sealed class AutomaticAccountingRulesService : IAutomaticAccountingRulesService
{
    private readonly FinanceProDbContext _db;
    public AutomaticAccountingRulesService(FinanceProDbContext db)=>_db=db;

    public async Task<IReadOnlyList<AutomaticAccountingRule>> ListAsync(int companyId,CancellationToken cancellationToken=default)
    {
        var result=new List<AutomaticAccountingRule>(); var cn=_db.Database.GetDbConnection(); var close=cn.State!=ConnectionState.Open;
        if(close) await cn.OpenAsync(cancellationToken);
        try { await using var cmd=cn.CreateCommand(); cmd.CommandText="SELECT Id,CompanyId,EventCode,DebitAccountId,CreditAccountId,Active,Description FROM dbo.AutomaticAccountingRules WHERE CompanyId=@c ORDER BY EventCode"; Add(cmd,"@c",companyId);
            await using var rd=await cmd.ExecuteReaderAsync(cancellationToken); while(await rd.ReadAsync(cancellationToken)) result.Add(new(rd.GetInt32(0),rd.GetInt32(1),rd.GetString(2),rd.GetInt32(3),rd.GetInt32(4),rd.GetBoolean(5),rd.GetString(6))); return result; }
        finally { if(close) await cn.CloseAsync(); }
    }

    public async Task SaveAsync(SaveAutomaticAccountingRuleRequest r,CancellationToken cancellationToken=default)
    {
        if(r.DebitAccountId<=0||r.CreditAccountId<=0) throw new InvalidOperationException("Selecione as contas de débito e crédito.");
        if(r.DebitAccountId==r.CreditAccountId) throw new InvalidOperationException("Débito e crédito devem usar contas diferentes.");
        var cn=_db.Database.GetDbConnection(); var close=cn.State!=ConnectionState.Open; if(close) await cn.OpenAsync(cancellationToken);
        try { await using var cmd=cn.CreateCommand(); cmd.CommandText=@"MERGE dbo.AutomaticAccountingRules AS t USING (SELECT @CompanyId CompanyId,@EventCode EventCode) s ON t.CompanyId=s.CompanyId AND t.EventCode=s.EventCode
WHEN MATCHED THEN UPDATE SET DebitAccountId=@Debit,CreditAccountId=@Credit,Active=@Active,Description=@Description
WHEN NOT MATCHED THEN INSERT(CompanyId,EventCode,DebitAccountId,CreditAccountId,Active,Description) VALUES(@CompanyId,@EventCode,@Debit,@Credit,@Active,@Description);";
            Add(cmd,"@CompanyId",r.CompanyId);Add(cmd,"@EventCode",r.EventCode);Add(cmd,"@Debit",r.DebitAccountId);Add(cmd,"@Credit",r.CreditAccountId);Add(cmd,"@Active",r.Active);Add(cmd,"@Description",r.Description); await cmd.ExecuteNonQueryAsync(cancellationToken); }
        finally { if(close) await cn.CloseAsync(); }
    }
    private static void Add(System.Data.Common.DbCommand c,string n,object? v){var p=c.CreateParameter();p.ParameterName=n;p.Value=v??DBNull.Value;c.Parameters.Add(p);}
}

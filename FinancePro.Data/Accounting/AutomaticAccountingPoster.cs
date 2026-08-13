using FinancePro.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace FinancePro.Data.Accounting;

/// <summary>Posts balanced, idempotent entries for operational events when an accounting rule is configured.</summary>
public static class AutomaticAccountingPoster
{
    public static async Task TryPostAsync(FinanceProDbContext db, int companyId, string eventCode, int sourceId,
        DateTime date, string documentNumber, string reference, string description, decimal amount)
    {
        if (companyId <= 0 || sourceId <= 0 || amount <= 0) return;
        await db.Database.ExecuteSqlInterpolatedAsync($@"
DECLARE @Debit int,@Credit int,@EntryId int;
SELECT @Debit=DebitAccountId,@Credit=CreditAccountId FROM dbo.AutomaticAccountingRules
 WHERE CompanyId={companyId} AND EventCode={eventCode} AND Active=1;
IF @Debit IS NOT NULL AND @Credit IS NOT NULL
 AND NOT EXISTS(SELECT 1 FROM dbo.AutomaticAccountingLinks WHERE CompanyId={companyId} AND EventCode={eventCode} AND SourceId={sourceId})
BEGIN
 INSERT dbo.AccountingEntries(CompanyId,EntryDate,DocumentNumber,Reference,Description,SourceModule,Status,CreatedBy,CreatedByName,PostedBy,PostedByName,PostedAt,CreatedAt,UpdatedAt)
 VALUES({companyId},{date.Date},{documentNumber},{reference},{description},{eventCode},'Contabilizado',0,'Sistema',0,'Sistema',SYSUTCDATETIME(),SYSUTCDATETIME(),SYSUTCDATETIME());
 SET @EntryId=CAST(SCOPE_IDENTITY() AS int);
 INSERT dbo.AccountingEntryLines(AccountingEntryId,AccountId,CostCenterId,Description,Debit,Credit)
 VALUES(@EntryId,@Debit,NULL,{description},{amount},0),(@EntryId,@Credit,NULL,{description},0,{amount});
 INSERT dbo.AutomaticAccountingLinks(CompanyId,EventCode,SourceId,AccountingEntryId) VALUES({companyId},{eventCode},{sourceId},@EntryId);
END");
    }
}

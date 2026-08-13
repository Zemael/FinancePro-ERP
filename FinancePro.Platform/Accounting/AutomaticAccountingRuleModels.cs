namespace FinancePro.Platform.Accounting;

public sealed record AutomaticAccountingRule(int Id,int CompanyId,string EventCode,int DebitAccountId,int CreditAccountId,bool Active,string Description);
public sealed record SaveAutomaticAccountingRuleRequest(int CompanyId,string EventCode,int DebitAccountId,int CreditAccountId,bool Active,string Description);

public interface IAutomaticAccountingRulesService
{
    Task<IReadOnlyList<AutomaticAccountingRule>> ListAsync(int companyId, CancellationToken cancellationToken=default);
    Task SaveAsync(SaveAutomaticAccountingRuleRequest request, CancellationToken cancellationToken=default);
}

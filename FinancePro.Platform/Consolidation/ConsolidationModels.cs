namespace FinancePro.Platform.Consolidation;

public static class ConsolidationRunStatus
{
    public const string Draft = "Rascunho";
    public const string Validated = "Validado";
    public const string Consolidated = "Consolidado";
    public const string Closed = "Fechado";
}

public sealed record ConsolidationCompany(int Id, string Name);
public sealed record ConsolidationGroup(int Id, string Name, int FiscalYear, IReadOnlyList<int> CompanyIds, DateTime CreatedAt, DateTime UpdatedAt);
public sealed record SaveConsolidationGroupRequest(int? Id, string Name, int FiscalYear, IReadOnlyList<int> CompanyIds);
public sealed record ConsolidationRun(int Id, int GroupId, string GroupName, DateTime FromDate, DateTime ToDate, string Status, int CreatedBy, string CreatedByName, DateTime CreatedAt, DateTime? ValidatedAt, DateTime? ConsolidatedAt, DateTime? ClosedAt);
public sealed record CreateConsolidationRunRequest(int GroupId, DateTime FromDate, DateTime ToDate, int UserId, string UserName);

public sealed record ConsolidatedTrialBalanceRow(
    string AccountCode,
    string AccountName,
    decimal SourceDebit,
    decimal SourceCredit,
    decimal EliminationDebit,
    decimal EliminationCredit,
    decimal ConsolidatedDebit,
    decimal ConsolidatedCredit)
{
    public decimal NetBalance => ConsolidatedDebit - ConsolidatedCredit;
}

public sealed record IntercompanyDifference(
    string Reference,
    int CompanyCount,
    string Companies,
    decimal MinimumAmount,
    decimal MaximumAmount,
    decimal Difference)
{
    public bool IsReconciled => CompanyCount >= 2 && Math.Abs(Difference) < 0.01m;
}

public sealed record ConsolidationElimination(
    int Id,
    int RunId,
    string AccountCode,
    string Description,
    decimal Debit,
    decimal Credit,
    string Reference,
    int CreatedBy,
    string CreatedByName,
    DateTime CreatedAt);

public sealed record SaveConsolidationEliminationRequest(
    int RunId,
    string AccountCode,
    string Description,
    decimal Debit,
    decimal Credit,
    string Reference,
    int UserId,
    string UserName);

public sealed record ConsolidationValidationCheck(string Code, string Description, bool IsBlocking, int Count, string Message);
public sealed record ConsolidationValidationResult(int RunId, IReadOnlyList<ConsolidationValidationCheck> Checks)
{
    public bool CanValidate => Checks.All(x => !x.IsBlocking || x.Count == 0);
    public int BlockingIssues => Checks.Where(x => x.IsBlocking).Sum(x => x.Count);
}

public sealed record ConsolidationSummary(
    int CompanyCount,
    int AccountCount,
    decimal SourceDebit,
    decimal SourceCredit,
    decimal EliminationDebit,
    decimal EliminationCredit,
    decimal ConsolidatedDebit,
    decimal ConsolidatedCredit,
    int IntercompanyDifferences)
{
    public bool IsBalanced => Math.Abs(ConsolidatedDebit - ConsolidatedCredit) < 0.01m;
}

namespace FinancePro.Platform.Administration;

public sealed record CostCenter(int Id, int CompanyId, string Code, string Name, bool Active);
public sealed record TaxRate(int Id, int CompanyId, string Code, string Name, decimal Rate, bool Active);
public sealed record ChartAccount(int Id, int CompanyId, string Code, string Name, int? ParentId, string AccountType, string Nature, bool AllowsPosting, bool RequiresCostCenter, bool Active);
public sealed record DocumentSequence(int Id, int CompanyId, int FiscalYear, string Module, string Prefix, long CurrentNumber, int Digits, bool RestartAnnually, bool Active);
public sealed record FiscalObligation(int Id, int CompanyId, string Code, string Name, string Category, DateTime DueDate, string Frequency, string Status, string? Notes, bool Active)
{
    public bool IsOverdue => Active && Status == "Pendente" && DueDate.Date < DateTime.Today;
    public bool IsDueSoon => Active && Status == "Pendente" && DueDate.Date >= DateTime.Today && DueDate.Date <= DateTime.Today.AddDays(7);
}

public sealed record AccountingPeriod(int Id, int CompanyId, int FiscalYear, int Month, DateTime StartDate, DateTime EndDate, string Status, bool Active);

public sealed record FiscalComplianceSummary(
    int TotalActive,
    int Pending,
    int Overdue,
    int DueSoon,
    int Completed,
    decimal ComplianceRate);

public sealed record SaveCostCenterRequest(int CompanyId, int? Id, string Code, string Name, bool Active = true);
public sealed record SaveTaxRateRequest(int CompanyId, int? Id, string Code, string Name, decimal Rate, bool Active = true);
public sealed record SaveChartAccountRequest(int CompanyId, int? Id, string Code, string Name, int? ParentId, string AccountType, string Nature, bool AllowsPosting, bool RequiresCostCenter, bool Active = true);
public sealed record SaveDocumentSequenceRequest(int CompanyId, int? Id, int FiscalYear, string Module, string Prefix, long CurrentNumber, int Digits, bool RestartAnnually, bool Active = true);
public sealed record SaveAccountingPeriodRequest(int CompanyId, int? Id, int FiscalYear, int Month, DateTime StartDate, DateTime EndDate, string Status, bool Active = true);
public sealed record SaveFiscalObligationRequest(int CompanyId, int? Id, string Code, string Name, string Category, DateTime DueDate, string Frequency, string Status, string? Notes, bool Active = true);

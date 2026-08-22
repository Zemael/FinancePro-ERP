namespace FinancePro.Platform.Accounting;

public sealed record Department(int Id,int CompanyId,string Code,string Name,bool Active);
public sealed record AnalyticCostCenter(int Id,int CompanyId,string Code,string Name,int? DepartmentId,string? DepartmentName,bool Active);
public sealed record CostCenterPerformanceRow(int CostCenterId,string CostCenterCode,string CostCenterName,string Department,decimal Budgeted,decimal Committed,decimal Realized,decimal Revenue,decimal Expense,decimal Result,decimal Variance,decimal ExecutionRate);
public sealed record AnalyticAccountingSummary(decimal Budgeted,decimal Committed,decimal Realized,decimal Revenue,decimal Expense,decimal Result,decimal Variance);
public sealed record SaveDepartmentRequest(int CompanyId,int? Id,string Code,string Name,bool Active=true);

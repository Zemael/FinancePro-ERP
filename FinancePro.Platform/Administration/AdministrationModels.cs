namespace FinancePro.Platform.Administration;

public sealed record CostCenter(int Id, int CompanyId, string Code, string Name, bool Active);
public sealed record TaxRate(int Id, int CompanyId, string Code, string Name, decimal Rate, bool Active);
public sealed record SaveCostCenterRequest(int CompanyId, int? Id, string Code, string Name, bool Active = true);
public sealed record SaveTaxRateRequest(int CompanyId, int? Id, string Code, string Name, decimal Rate, bool Active = true);

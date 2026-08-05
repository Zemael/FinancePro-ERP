namespace FinancePro.Platform.Administration;

public sealed record CostCenter(int Id, int CompanyId, string Code, string Name, bool Active);
public sealed record TaxRate(int Id, int CompanyId, string Code, string Name, decimal Rate, bool Active);
public sealed record ChartAccount(int Id, int CompanyId, string Code, string Name, int? ParentId, string AccountType, string Nature, bool AllowsPosting, bool RequiresCostCenter, bool Active);
public sealed record DocumentSequence(int Id, int CompanyId, int FiscalYear, string Module, string Prefix, long CurrentNumber, int Digits, bool RestartAnnually, bool Active);

public sealed record SaveCostCenterRequest(int CompanyId, int? Id, string Code, string Name, bool Active = true);
public sealed record SaveTaxRateRequest(int CompanyId, int? Id, string Code, string Name, decimal Rate, bool Active = true);
public sealed record SaveChartAccountRequest(int CompanyId, int? Id, string Code, string Name, int? ParentId, string AccountType, string Nature, bool AllowsPosting, bool RequiresCostCenter, bool Active = true);
public sealed record SaveDocumentSequenceRequest(int CompanyId, int? Id, int FiscalYear, string Module, string Prefix, long CurrentNumber, int Digits, bool RestartAnnually, bool Active = true);

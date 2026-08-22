namespace FinancePro.Platform.Accounting;
public sealed record ProjectRow(int Id,string Code,string Name,string Client,string CostCenter,DateTime StartDate,DateTime? EndDate,decimal BudgetRevenue,decimal BudgetCost,string Status,decimal Revenue,decimal Cost,decimal Result,decimal Margin);
public sealed record SaveProjectRequest(int CompanyId,int? Id,string Code,string Name,int? ClientId,int? CostCenterId,DateTime StartDate,DateTime? EndDate,decimal BudgetRevenue,decimal BudgetCost,string Status="Planeado");
public sealed record ProjectClientOption(int Id,string Name);
public sealed record WorkOrderRow(int Id,int ProjectId,string Project,string Number,string Description,DateTime OrderDate,DateTime? DueDate,string Status,decimal EstimatedCost,decimal ActualCost);
public sealed record SaveWorkOrderRequest(int CompanyId,int? Id,int ProjectId,string Number,string Description,DateTime OrderDate,DateTime? DueDate,decimal EstimatedCost,string Status="Aberta");

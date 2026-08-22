namespace FinancePro.Platform.Accounting;

public interface IAnalyticAccountingService
{
    Task<IReadOnlyList<Department>> ListDepartmentsAsync(int companyId,CancellationToken cancellationToken=default);
    Task<IReadOnlyList<AnalyticCostCenter>> ListCostCentersAsync(int companyId,CancellationToken cancellationToken=default);
    Task SaveDepartmentAsync(SaveDepartmentRequest request,CancellationToken cancellationToken=default);
    Task SetDepartmentActiveAsync(int companyId,int id,bool active,CancellationToken cancellationToken=default);
    Task AssignCostCenterDepartmentAsync(int companyId,int costCenterId,int? departmentId,CancellationToken cancellationToken=default);
    Task<IReadOnlyList<CostCenterPerformanceRow>> GetPerformanceAsync(int companyId,DateTime from,DateTime to,CancellationToken cancellationToken=default);
    Task<AnalyticAccountingSummary> GetSummaryAsync(int companyId,DateTime from,DateTime to,CancellationToken cancellationToken=default);
    Task<IReadOnlyList<ProjectRow>> ListProjectsAsync(int companyId,CancellationToken cancellationToken=default);
    Task SaveProjectAsync(SaveProjectRequest request,CancellationToken cancellationToken=default);
    Task<IReadOnlyList<ProjectClientOption>> ListProjectClientsAsync(int companyId,CancellationToken cancellationToken=default);
    Task<IReadOnlyList<WorkOrderRow>> ListWorkOrdersAsync(int companyId,int? projectId=null,CancellationToken cancellationToken=default);
    Task SaveWorkOrderAsync(SaveWorkOrderRequest request,CancellationToken cancellationToken=default);
}

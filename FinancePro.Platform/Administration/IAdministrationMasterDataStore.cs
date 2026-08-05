namespace FinancePro.Platform.Administration;

public interface IAdministrationMasterDataStore
{
    Task<IReadOnlyList<CostCenter>> ListCostCentersAsync(int companyId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TaxRate>> ListTaxRatesAsync(int companyId, CancellationToken cancellationToken = default);
    Task SaveCostCenterAsync(SaveCostCenterRequest request, CancellationToken cancellationToken = default);
    Task SaveTaxRateAsync(SaveTaxRateRequest request, CancellationToken cancellationToken = default);
    Task SetCostCenterActiveAsync(int companyId, int id, bool active, CancellationToken cancellationToken = default);
    Task SetTaxRateActiveAsync(int companyId, int id, bool active, CancellationToken cancellationToken = default);
}

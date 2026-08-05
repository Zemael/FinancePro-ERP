namespace FinancePro.Platform.Administration;

public interface IAdministrationMasterDataStore
{
    Task<IReadOnlyList<CostCenter>> ListCostCentersAsync(int companyId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TaxRate>> ListTaxRatesAsync(int companyId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ChartAccount>> ListChartAccountsAsync(int companyId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DocumentSequence>> ListDocumentSequencesAsync(int companyId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<FiscalObligation>> ListFiscalObligationsAsync(int companyId, CancellationToken cancellationToken = default);
    Task SaveCostCenterAsync(SaveCostCenterRequest request, CancellationToken cancellationToken = default);
    Task SaveTaxRateAsync(SaveTaxRateRequest request, CancellationToken cancellationToken = default);
    Task SaveChartAccountAsync(SaveChartAccountRequest request, CancellationToken cancellationToken = default);
    Task SaveDocumentSequenceAsync(SaveDocumentSequenceRequest request, CancellationToken cancellationToken = default);
    Task SaveFiscalObligationAsync(SaveFiscalObligationRequest request, CancellationToken cancellationToken = default);
    Task SetCostCenterActiveAsync(int companyId, int id, bool active, CancellationToken cancellationToken = default);
    Task SetTaxRateActiveAsync(int companyId, int id, bool active, CancellationToken cancellationToken = default);
    Task SetChartAccountActiveAsync(int companyId, int id, bool active, CancellationToken cancellationToken = default);
    Task SetDocumentSequenceActiveAsync(int companyId, int id, bool active, CancellationToken cancellationToken = default);
    Task SetFiscalObligationActiveAsync(int companyId, int id, bool active, CancellationToken cancellationToken = default);
}

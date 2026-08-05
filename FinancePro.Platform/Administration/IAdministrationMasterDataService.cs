namespace FinancePro.Platform.Administration;

public interface IAdministrationMasterDataService
{
    Task<IReadOnlyList<CostCenter>> ListCostCentersAsync(int companyId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TaxRate>> ListTaxRatesAsync(int companyId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ChartAccount>> ListChartAccountsAsync(int companyId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DocumentSequence>> ListDocumentSequencesAsync(int companyId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AccountingPeriod>> ListAccountingPeriodsAsync(int companyId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<FiscalObligation>> ListFiscalObligationsAsync(int companyId, CancellationToken cancellationToken = default);
    Task<FiscalComplianceSummary> GetFiscalComplianceSummaryAsync(int companyId, DateTime? referenceDate = null, CancellationToken cancellationToken = default);
    Task SaveCostCenterAsync(SaveCostCenterRequest request, CancellationToken cancellationToken = default);
    Task SaveTaxRateAsync(SaveTaxRateRequest request, CancellationToken cancellationToken = default);
    Task SaveChartAccountAsync(SaveChartAccountRequest request, CancellationToken cancellationToken = default);
    Task SaveDocumentSequenceAsync(SaveDocumentSequenceRequest request, CancellationToken cancellationToken = default);
    Task SaveAccountingPeriodAsync(SaveAccountingPeriodRequest request, CancellationToken cancellationToken = default);
    Task SaveFiscalObligationAsync(SaveFiscalObligationRequest request, CancellationToken cancellationToken = default);
    Task SetCostCenterActiveAsync(int companyId, int id, bool active, CancellationToken cancellationToken = default);
    Task SetTaxRateActiveAsync(int companyId, int id, bool active, CancellationToken cancellationToken = default);
    Task SetChartAccountActiveAsync(int companyId, int id, bool active, CancellationToken cancellationToken = default);
    Task SetDocumentSequenceActiveAsync(int companyId, int id, bool active, CancellationToken cancellationToken = default);
    Task SetAccountingPeriodActiveAsync(int companyId, int id, bool active, CancellationToken cancellationToken = default);
    Task SetAccountingPeriodStatusAsync(int companyId, int id, string status, CancellationToken cancellationToken = default);
    Task SetFiscalObligationActiveAsync(int companyId, int id, bool active, CancellationToken cancellationToken = default);
    Task SetFiscalObligationStatusAsync(int companyId, int id, string status, CancellationToken cancellationToken = default);
}

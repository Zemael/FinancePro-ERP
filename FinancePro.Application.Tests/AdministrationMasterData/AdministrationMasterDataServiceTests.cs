using FinancePro.Platform.Administration;
using Xunit;

namespace FinancePro.Application.Tests.AdministrationMasterData;

public sealed class AdministrationMasterDataServiceTests
{
    [Fact]
    public async Task SaveTaxRate_normalizes_code_and_validates_rate()
    {
        var store = new FakeStore();
        var service = new AdministrationMasterDataService(store);
        await service.SaveTaxRateAsync(new SaveTaxRateRequest(1, null, " iva19 ", " IVA normal ", 19));
        Assert.Equal("IVA19", store.LastTax!.Code);
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => service.SaveTaxRateAsync(new SaveTaxRateRequest(1, null, "X", "Inválida", 101)));
    }

    [Fact]
    public async Task SaveFiscalObligation_normalizes_code_and_validates_category()
    {
        var store = new FakeStore();
        var service = new AdministrationMasterDataService(store);
        await service.SaveFiscalObligationAsync(new SaveFiscalObligationRequest(1, null, " iva-ago ", "IVA de agosto", "IVA", new DateTime(2026, 9, 15), "Mensal", "Pendente", null));
        Assert.Equal("IVA-AGO", store.LastFiscal!.Code);
        await Assert.ThrowsAsync<ArgumentException>(() => service.SaveFiscalObligationAsync(new SaveFiscalObligationRequest(1, null, "X", "Inválida", "Desconhecida", DateTime.Today, "Mensal", "Pendente", null)));
    }

    private sealed class FakeStore : IAdministrationMasterDataStore
    {
        public SaveTaxRateRequest? LastTax { get; private set; } public SaveFiscalObligationRequest? LastFiscal { get; private set; }
        public Task<IReadOnlyList<CostCenter>> ListCostCentersAsync(int companyId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<CostCenter>>([]);
        public Task<IReadOnlyList<TaxRate>> ListTaxRatesAsync(int companyId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<TaxRate>>([]);
        public Task<IReadOnlyList<ChartAccount>> ListChartAccountsAsync(int companyId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<ChartAccount>>([]);
        public Task<IReadOnlyList<DocumentSequence>> ListDocumentSequencesAsync(int companyId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<DocumentSequence>>([]);
        public Task<IReadOnlyList<FiscalObligation>> ListFiscalObligationsAsync(int companyId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<FiscalObligation>>([]);
        public Task SaveCostCenterAsync(SaveCostCenterRequest request, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task SaveTaxRateAsync(SaveTaxRateRequest request, CancellationToken cancellationToken = default) { LastTax = request; return Task.CompletedTask; }
        public Task SaveChartAccountAsync(SaveChartAccountRequest request, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task SaveDocumentSequenceAsync(SaveDocumentSequenceRequest request, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task SaveFiscalObligationAsync(SaveFiscalObligationRequest request, CancellationToken cancellationToken = default) { LastFiscal = request; return Task.CompletedTask; }
        public Task SetCostCenterActiveAsync(int companyId, int id, bool active, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task SetTaxRateActiveAsync(int companyId, int id, bool active, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task SetChartAccountActiveAsync(int companyId, int id, bool active, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task SetDocumentSequenceActiveAsync(int companyId, int id, bool active, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task SetFiscalObligationActiveAsync(int companyId, int id, bool active, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}

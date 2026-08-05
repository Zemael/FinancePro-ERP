namespace FinancePro.Platform.Administration;

public sealed class AdministrationMasterDataService : IAdministrationMasterDataService
{
    private readonly IAdministrationMasterDataStore _store;
    public AdministrationMasterDataService(IAdministrationMasterDataStore store) => _store = store;

    public Task<IReadOnlyList<CostCenter>> ListCostCentersAsync(int companyId, CancellationToken cancellationToken = default) =>
        companyId > 0 ? _store.ListCostCentersAsync(companyId, cancellationToken) : throw new ArgumentOutOfRangeException(nameof(companyId));

    public Task<IReadOnlyList<TaxRate>> ListTaxRatesAsync(int companyId, CancellationToken cancellationToken = default) =>
        companyId > 0 ? _store.ListTaxRatesAsync(companyId, cancellationToken) : throw new ArgumentOutOfRangeException(nameof(companyId));

    public Task SaveCostCenterAsync(SaveCostCenterRequest request, CancellationToken cancellationToken = default)
    {
        Validate(request.CompanyId, request.Code, request.Name);
        return _store.SaveCostCenterAsync(request with { Code = request.Code.Trim().ToUpperInvariant(), Name = request.Name.Trim() }, cancellationToken);
    }

    public Task SaveTaxRateAsync(SaveTaxRateRequest request, CancellationToken cancellationToken = default)
    {
        Validate(request.CompanyId, request.Code, request.Name);
        if (request.Rate is < 0 or > 100) throw new ArgumentOutOfRangeException(nameof(request), "A taxa deve estar entre 0 e 100.");
        return _store.SaveTaxRateAsync(request with { Code = request.Code.Trim().ToUpperInvariant(), Name = request.Name.Trim() }, cancellationToken);
    }

    public Task SetCostCenterActiveAsync(int companyId, int id, bool active, CancellationToken cancellationToken = default) =>
        _store.SetCostCenterActiveAsync(companyId, id, active, cancellationToken);

    public Task SetTaxRateActiveAsync(int companyId, int id, bool active, CancellationToken cancellationToken = default) =>
        _store.SetTaxRateActiveAsync(companyId, id, active, cancellationToken);

    private static void Validate(int companyId, string code, string name)
    {
        if (companyId <= 0) throw new ArgumentOutOfRangeException(nameof(companyId));
        if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException("Código obrigatório.", nameof(code));
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Nome obrigatório.", nameof(name));
    }
}

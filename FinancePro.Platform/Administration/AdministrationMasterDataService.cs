namespace FinancePro.Platform.Administration;

public sealed class AdministrationMasterDataService : IAdministrationMasterDataService
{
    private readonly IAdministrationMasterDataStore _store;
    public AdministrationMasterDataService(IAdministrationMasterDataStore store) => _store = store;

    public Task<IReadOnlyList<CostCenter>> ListCostCentersAsync(int companyId, CancellationToken cancellationToken = default) => ValidateCompanyAndRun(companyId, () => _store.ListCostCentersAsync(companyId, cancellationToken));
    public Task<IReadOnlyList<TaxRate>> ListTaxRatesAsync(int companyId, CancellationToken cancellationToken = default) => ValidateCompanyAndRun(companyId, () => _store.ListTaxRatesAsync(companyId, cancellationToken));
    public Task<IReadOnlyList<ChartAccount>> ListChartAccountsAsync(int companyId, CancellationToken cancellationToken = default) => ValidateCompanyAndRun(companyId, () => _store.ListChartAccountsAsync(companyId, cancellationToken));
    public Task<IReadOnlyList<DocumentSequence>> ListDocumentSequencesAsync(int companyId, CancellationToken cancellationToken = default) => ValidateCompanyAndRun(companyId, () => _store.ListDocumentSequencesAsync(companyId, cancellationToken));

    public Task SaveCostCenterAsync(SaveCostCenterRequest request, CancellationToken cancellationToken = default)
    { Validate(request.CompanyId, request.Code, request.Name); return _store.SaveCostCenterAsync(request with { Code=request.Code.Trim().ToUpperInvariant(), Name=request.Name.Trim() }, cancellationToken); }
    public Task SaveTaxRateAsync(SaveTaxRateRequest request, CancellationToken cancellationToken = default)
    { Validate(request.CompanyId, request.Code, request.Name); if(request.Rate is < 0 or > 100) throw new ArgumentOutOfRangeException(nameof(request),"A taxa deve estar entre 0 e 100."); return _store.SaveTaxRateAsync(request with { Code=request.Code.Trim().ToUpperInvariant(), Name=request.Name.Trim() }, cancellationToken); }
    public Task SaveChartAccountAsync(SaveChartAccountRequest request, CancellationToken cancellationToken = default)
    {
        Validate(request.CompanyId, request.Code, request.Name);
        var type=request.AccountType.Trim(); var nature=request.Nature.Trim();
        if(type is not ("Sintética" or "Analítica")) throw new ArgumentException("Tipo de conta inválido.", nameof(request));
        if(nature is not ("Devedora" or "Credora")) throw new ArgumentException("Natureza contabilística inválida.", nameof(request));
        if(type=="Sintética" && request.AllowsPosting) throw new ArgumentException("Contas sintéticas não podem aceitar lançamentos.", nameof(request));
        return _store.SaveChartAccountAsync(request with { Code=request.Code.Trim(), Name=request.Name.Trim(), AccountType=type, Nature=nature }, cancellationToken);
    }
    public Task SaveDocumentSequenceAsync(SaveDocumentSequenceRequest request, CancellationToken cancellationToken = default)
    {
        if(request.CompanyId<=0) throw new ArgumentOutOfRangeException(nameof(request.CompanyId));
        if(request.FiscalYear is < 2000 or > 2200) throw new ArgumentOutOfRangeException(nameof(request.FiscalYear));
        if(string.IsNullOrWhiteSpace(request.Module)) throw new ArgumentException("Módulo obrigatório.", nameof(request));
        if(string.IsNullOrWhiteSpace(request.Prefix)) throw new ArgumentException("Prefixo obrigatório.", nameof(request));
        if(request.CurrentNumber<0) throw new ArgumentOutOfRangeException(nameof(request.CurrentNumber));
        if(request.Digits is < 3 or > 12) throw new ArgumentOutOfRangeException(nameof(request.Digits),"Os dígitos devem estar entre 3 e 12.");
        return _store.SaveDocumentSequenceAsync(request with { Module=request.Module.Trim().ToUpperInvariant(), Prefix=request.Prefix.Trim().ToUpperInvariant() }, cancellationToken);
    }

    public Task SetCostCenterActiveAsync(int companyId,int id,bool active,CancellationToken cancellationToken=default)=>_store.SetCostCenterActiveAsync(companyId,id,active,cancellationToken);
    public Task SetTaxRateActiveAsync(int companyId,int id,bool active,CancellationToken cancellationToken=default)=>_store.SetTaxRateActiveAsync(companyId,id,active,cancellationToken);
    public Task SetChartAccountActiveAsync(int companyId,int id,bool active,CancellationToken cancellationToken=default)=>_store.SetChartAccountActiveAsync(companyId,id,active,cancellationToken);
    public Task SetDocumentSequenceActiveAsync(int companyId,int id,bool active,CancellationToken cancellationToken=default)=>_store.SetDocumentSequenceActiveAsync(companyId,id,active,cancellationToken);

    private static Task<T> ValidateCompanyAndRun<T>(int companyId, Func<Task<T>> action) { if(companyId<=0) throw new ArgumentOutOfRangeException(nameof(companyId)); return action(); }
    private static void Validate(int companyId,string code,string name) { if(companyId<=0) throw new ArgumentOutOfRangeException(nameof(companyId)); if(string.IsNullOrWhiteSpace(code)) throw new ArgumentException("Código obrigatório.",nameof(code)); if(string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Nome obrigatório.",nameof(name)); }
}

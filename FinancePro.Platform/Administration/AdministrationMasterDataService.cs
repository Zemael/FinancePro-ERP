namespace FinancePro.Platform.Administration;

public sealed class AdministrationMasterDataService : IAdministrationMasterDataService
{
    private readonly IAdministrationMasterDataStore _store;
    public AdministrationMasterDataService(IAdministrationMasterDataStore store) => _store = store;

    public Task<IReadOnlyList<CostCenter>> ListCostCentersAsync(int companyId, CancellationToken cancellationToken = default) => ValidateCompanyAndRun(companyId, () => _store.ListCostCentersAsync(companyId, cancellationToken));
    public Task<IReadOnlyList<TaxRate>> ListTaxRatesAsync(int companyId, CancellationToken cancellationToken = default) => ValidateCompanyAndRun(companyId, () => _store.ListTaxRatesAsync(companyId, cancellationToken));
    public Task<IReadOnlyList<ChartAccount>> ListChartAccountsAsync(int companyId, CancellationToken cancellationToken = default) => ValidateCompanyAndRun(companyId, () => _store.ListChartAccountsAsync(companyId, cancellationToken));
    public Task<IReadOnlyList<DocumentSequence>> ListDocumentSequencesAsync(int companyId, CancellationToken cancellationToken = default) => ValidateCompanyAndRun(companyId, () => _store.ListDocumentSequencesAsync(companyId, cancellationToken));
    public Task<IReadOnlyList<AccountingPeriod>> ListAccountingPeriodsAsync(int companyId, CancellationToken cancellationToken = default) => ValidateCompanyAndRun(companyId, () => _store.ListAccountingPeriodsAsync(companyId, cancellationToken));
    public Task<IReadOnlyList<FiscalObligation>> ListFiscalObligationsAsync(int companyId, CancellationToken cancellationToken = default) => ValidateCompanyAndRun(companyId, () => _store.ListFiscalObligationsAsync(companyId, cancellationToken));

    public async Task<FiscalComplianceSummary> GetFiscalComplianceSummaryAsync(int companyId, DateTime? referenceDate = null, CancellationToken cancellationToken = default)
    {
        if (companyId <= 0) throw new ArgumentOutOfRangeException(nameof(companyId));
        var reference = (referenceDate ?? DateTime.Today).Date;
        var obligations = await _store.ListFiscalObligationsAsync(companyId, cancellationToken);
        var active = obligations.Where(x => x.Active && x.Status != "Cancelada").ToList();
        var completed = active.Count(x => x.Status == "Cumprida");
        var pending = active.Count(x => x.Status == "Pendente");
        var overdue = active.Count(x => x.Status == "Atrasada" || (x.Status == "Pendente" && x.DueDate.Date < reference));
        var dueSoon = active.Count(x => x.Status == "Pendente" && x.DueDate.Date >= reference && x.DueDate.Date <= reference.AddDays(7));
        var rate = active.Count == 0 ? 100m : Math.Round(completed * 100m / active.Count, 1);
        return new FiscalComplianceSummary(active.Count, pending, overdue, dueSoon, completed, rate);
    }

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

    public Task SaveAccountingPeriodAsync(SaveAccountingPeriodRequest request, CancellationToken cancellationToken = default)
    {
        if (request.CompanyId <= 0) throw new ArgumentOutOfRangeException(nameof(request.CompanyId));
        if (request.FiscalYear is < 2000 or > 2200) throw new ArgumentOutOfRangeException(nameof(request.FiscalYear));
        if (request.Month is < 1 or > 12) throw new ArgumentOutOfRangeException(nameof(request.Month));
        if (request.EndDate.Date < request.StartDate.Date) throw new ArgumentException("A data final não pode ser anterior à data inicial.", nameof(request));
        var statuses = new[] { "Aberto", "Fechado", "Bloqueado" };
        if (!statuses.Contains(request.Status)) throw new ArgumentException("Estado contabilístico inválido.", nameof(request));
        return _store.SaveAccountingPeriodAsync(request, cancellationToken);
    }

    public Task SaveFiscalObligationAsync(SaveFiscalObligationRequest request, CancellationToken cancellationToken = default)
    {
        Validate(request.CompanyId, request.Code, request.Name);
        var categories = new[] { "IVA", "Retenção", "Imposto", "Contribuição", "Outro" };
        var frequencies = new[] { "Única", "Mensal", "Trimestral", "Semestral", "Anual" };
        var statuses = new[] { "Pendente", "Cumprida", "Atrasada", "Cancelada" };
        if (!categories.Contains(request.Category)) throw new ArgumentException("Categoria fiscal inválida.", nameof(request));
        if (!frequencies.Contains(request.Frequency)) throw new ArgumentException("Periodicidade inválida.", nameof(request));
        if (!statuses.Contains(request.Status)) throw new ArgumentException("Estado inválido.", nameof(request));
        if (request.DueDate.Date < new DateTime(2000, 1, 1)) throw new ArgumentOutOfRangeException(nameof(request.DueDate));
        return _store.SaveFiscalObligationAsync(request with
        {
            Code = request.Code.Trim().ToUpperInvariant(), Name = request.Name.Trim(), Notes = request.Notes?.Trim()
        }, cancellationToken);
    }

    public Task SetCostCenterActiveAsync(int companyId,int id,bool active,CancellationToken cancellationToken=default)=>_store.SetCostCenterActiveAsync(companyId,id,active,cancellationToken);
    public Task SetTaxRateActiveAsync(int companyId,int id,bool active,CancellationToken cancellationToken=default)=>_store.SetTaxRateActiveAsync(companyId,id,active,cancellationToken);
    public Task SetChartAccountActiveAsync(int companyId,int id,bool active,CancellationToken cancellationToken=default)=>_store.SetChartAccountActiveAsync(companyId,id,active,cancellationToken);
    public Task SetDocumentSequenceActiveAsync(int companyId,int id,bool active,CancellationToken cancellationToken=default)=>_store.SetDocumentSequenceActiveAsync(companyId,id,active,cancellationToken);
    public Task SetAccountingPeriodActiveAsync(int companyId,int id,bool active,CancellationToken cancellationToken=default)=>_store.SetAccountingPeriodActiveAsync(companyId,id,active,cancellationToken);
    public Task SetAccountingPeriodStatusAsync(int companyId,int id,string status,CancellationToken cancellationToken=default)
    {
        if (companyId <= 0) throw new ArgumentOutOfRangeException(nameof(companyId));
        if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));
        var statuses = new[] { "Aberto", "Fechado", "Bloqueado" };
        if (!statuses.Contains(status)) throw new ArgumentException("Estado contabilístico inválido.", nameof(status));
        return _store.SetAccountingPeriodStatusAsync(companyId,id,status,cancellationToken);
    }
    public Task SetFiscalObligationActiveAsync(int companyId,int id,bool active,CancellationToken cancellationToken=default)=>_store.SetFiscalObligationActiveAsync(companyId,id,active,cancellationToken);
    public Task SetFiscalObligationStatusAsync(int companyId, int id, string status, CancellationToken cancellationToken = default)
    {
        if (companyId <= 0) throw new ArgumentOutOfRangeException(nameof(companyId));
        if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));
        var statuses = new[] { "Pendente", "Cumprida", "Atrasada", "Cancelada" };
        if (!statuses.Contains(status)) throw new ArgumentException("Estado fiscal inválido.", nameof(status));
        return _store.SetFiscalObligationStatusAsync(companyId, id, status, cancellationToken);
    }

    private static Task<T> ValidateCompanyAndRun<T>(int companyId, Func<Task<T>> action) { if(companyId<=0) throw new ArgumentOutOfRangeException(nameof(companyId)); return action(); }
    private static void Validate(int companyId,string code,string name) { if(companyId<=0) throw new ArgumentOutOfRangeException(nameof(companyId)); if(string.IsNullOrWhiteSpace(code)) throw new ArgumentException("Código obrigatório.",nameof(code)); if(string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Nome obrigatório.",nameof(name)); }
}

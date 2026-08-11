namespace FinancePro.Platform.Consolidation;

public sealed class ConsolidationService : IConsolidationService
{
    private readonly IConsolidationStore _store;
    public ConsolidationService(IConsolidationStore store) => _store = store;

    public Task<IReadOnlyList<ConsolidationCompany>> ListCompaniesAsync(CancellationToken cancellationToken = default) => _store.ListCompaniesAsync(cancellationToken);
    public Task<IReadOnlyList<ConsolidationGroup>> ListGroupsAsync(CancellationToken cancellationToken = default) => _store.ListGroupsAsync(cancellationToken);

    public Task<int> SaveGroupAsync(SaveConsolidationGroupRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name)) throw new ArgumentException("Nome do grupo obrigatório.");
        if (request.FiscalYear is < 2000 or > 2200) throw new ArgumentException("Exercício inválido.");
        if (request.CompanyIds.Distinct().Count() < 2) throw new InvalidOperationException("A consolidação exige pelo menos duas empresas.");
        return _store.SaveGroupAsync(request with { Name = request.Name.Trim(), CompanyIds = request.CompanyIds.Distinct().ToList() }, cancellationToken);
    }

    public Task<IReadOnlyList<ConsolidationRun>> ListRunsAsync(int groupId, CancellationToken cancellationToken = default) => _store.ListRunsAsync(groupId, cancellationToken);

    public Task<int> CreateRunAsync(CreateConsolidationRunRequest request, CancellationToken cancellationToken = default)
    {
        if (request.GroupId <= 0) throw new ArgumentException("Grupo de consolidação obrigatório.");
        if (request.FromDate.Date > request.ToDate.Date) throw new ArgumentException("Período de consolidação inválido.");
        if (request.UserId <= 0) throw new ArgumentException("Utilizador inválido.");
        return _store.CreateRunAsync(request with { FromDate = request.FromDate.Date, ToDate = request.ToDate.Date, UserName = request.UserName.Trim() }, cancellationToken);
    }

    public Task<IReadOnlyList<ConsolidatedTrialBalanceRow>> GetTrialBalanceAsync(int runId, CancellationToken cancellationToken = default) => _store.GetTrialBalanceAsync(runId, cancellationToken);
    public Task<IReadOnlyList<IntercompanyDifference>> GetIntercompanyDifferencesAsync(int runId, CancellationToken cancellationToken = default) => _store.GetIntercompanyDifferencesAsync(runId, cancellationToken);
    public Task<IReadOnlyList<ConsolidationElimination>> ListEliminationsAsync(int runId, CancellationToken cancellationToken = default) => _store.ListEliminationsAsync(runId, cancellationToken);

    public async Task<int> SaveEliminationAsync(SaveConsolidationEliminationRequest request, CancellationToken cancellationToken = default)
    {
        var run = await RequiredRunAsync(request.RunId, cancellationToken);
        if (run.Status is ConsolidationRunStatus.Consolidated or ConsolidationRunStatus.Closed)
            throw new InvalidOperationException("Não é possível alterar eliminações após a consolidação.");
        if (string.IsNullOrWhiteSpace(request.AccountCode)) throw new ArgumentException("Conta contabilística obrigatória.");
        if (request.Debit < 0 || request.Credit < 0 || (request.Debit == 0 && request.Credit == 0) || (request.Debit > 0 && request.Credit > 0))
            throw new ArgumentException("Informe apenas débito ou crédito com valor superior a zero.");
        return await _store.SaveEliminationAsync(request with { AccountCode = request.AccountCode.Trim(), Description = request.Description.Trim(), Reference = request.Reference.Trim() }, cancellationToken);
    }

    public async Task DeleteEliminationAsync(int runId, int eliminationId, CancellationToken cancellationToken = default)
    {
        var run = await RequiredRunAsync(runId, cancellationToken);
        if (run.Status is ConsolidationRunStatus.Consolidated or ConsolidationRunStatus.Closed)
            throw new InvalidOperationException("Não é possível alterar eliminações após a consolidação.");
        await _store.DeleteEliminationAsync(runId, eliminationId, cancellationToken);
    }

    public async Task<ConsolidationValidationResult> ValidateAsync(int runId, int userId, string userName, CancellationToken cancellationToken = default)
    {
        var run = await RequiredRunAsync(runId, cancellationToken);
        if (run.Status == ConsolidationRunStatus.Closed) throw new InvalidOperationException("A consolidação já está fechada.");

        var trial = await _store.GetTrialBalanceAsync(runId, cancellationToken);
        var differences = await _store.GetIntercompanyDifferencesAsync(runId, cancellationToken);
        var eliminations = await _store.ListEliminationsAsync(runId, cancellationToken);
        var groups = await _store.ListGroupsAsync(cancellationToken);
        var group = groups.FirstOrDefault(x => x.Id == run.GroupId) ?? throw new KeyNotFoundException("Grupo de consolidação não encontrado.");

        var checks = new List<ConsolidationValidationCheck>();
        var companyIssue = group.CompanyIds.Distinct().Count() < 2 ? 1 : 0;
        checks.Add(new("COMPANIES", "Empresas participantes", true, companyIssue, companyIssue == 0 ? $"{group.CompanyIds.Count} empresas incluídas." : "São necessárias pelo menos duas empresas."));

        var unbalanced = Math.Abs(trial.Sum(x => x.ConsolidatedDebit) - trial.Sum(x => x.ConsolidatedCredit)) >= 0.01m ? 1 : 0;
        checks.Add(new("BALANCE", "Equilíbrio do balancete consolidado", true, unbalanced, unbalanced == 0 ? "Débitos e créditos consolidados estão equilibrados." : "O balancete consolidado está desequilibrado."));

        var diffCount = differences.Count(x => !x.IsReconciled);
        checks.Add(new("INTERCOMPANY", "Reconciliação intercompany", true, diffCount, diffCount == 0 ? "Referências intercompany conciliadas." : $"Existem {diffCount} referência(s) intercompany com divergência."));

        var elimDebit = eliminations.Sum(x => x.Debit); var elimCredit = eliminations.Sum(x => x.Credit);
        var elimIssue = Math.Abs(elimDebit - elimCredit) >= 0.01m ? 1 : 0;
        checks.Add(new("ELIMINATIONS", "Eliminações contabilísticas", true, elimIssue, elimIssue == 0 ? "Eliminações equilibradas." : $"Eliminações desequilibradas em {Math.Abs(elimDebit - elimCredit):N2}."));

        var result = new ConsolidationValidationResult(runId, checks);
        if (result.CanValidate)
            await _store.SetRunStatusAsync(runId, ConsolidationRunStatus.Validated, userId, userName, cancellationToken);
        return result;
    }

    public async Task ConsolidateAsync(int runId, int userId, string userName, CancellationToken cancellationToken = default)
    {
        var run = await RequiredRunAsync(runId, cancellationToken);
        if (run.Status != ConsolidationRunStatus.Validated) throw new InvalidOperationException("A consolidação deve estar validada antes do processamento final.");
        var summary = await GetSummaryAsync(runId, cancellationToken);
        if (!summary.IsBalanced) throw new InvalidOperationException("O balancete consolidado está desequilibrado.");
        if (summary.IntercompanyDifferences > 0) throw new InvalidOperationException("Existem divergências intercompany por resolver.");
        await _store.SetRunStatusAsync(runId, ConsolidationRunStatus.Consolidated, userId, userName, cancellationToken);
    }

    public async Task CloseAsync(int runId, int userId, string userName, CancellationToken cancellationToken = default)
    {
        var run = await RequiredRunAsync(runId, cancellationToken);
        if (run.Status != ConsolidationRunStatus.Consolidated) throw new InvalidOperationException("Apenas consolidações processadas podem ser fechadas.");
        await _store.SetRunStatusAsync(runId, ConsolidationRunStatus.Closed, userId, userName, cancellationToken);
    }

    public async Task<ConsolidationSummary> GetSummaryAsync(int runId, CancellationToken cancellationToken = default)
    {
        var run = await RequiredRunAsync(runId, cancellationToken);
        var rows = await _store.GetTrialBalanceAsync(runId, cancellationToken);
        var differences = await _store.GetIntercompanyDifferencesAsync(runId, cancellationToken);
        var groups = await _store.ListGroupsAsync(cancellationToken);
        var group = groups.First(x => x.Id == run.GroupId);
        return new ConsolidationSummary(group.CompanyIds.Distinct().Count(), rows.Count,
            rows.Sum(x => x.SourceDebit), rows.Sum(x => x.SourceCredit),
            rows.Sum(x => x.EliminationDebit), rows.Sum(x => x.EliminationCredit),
            rows.Sum(x => x.ConsolidatedDebit), rows.Sum(x => x.ConsolidatedCredit),
            differences.Count(x => !x.IsReconciled));
    }

    private async Task<ConsolidationRun> RequiredRunAsync(int runId, CancellationToken ct) =>
        await _store.GetRunAsync(runId, ct) ?? throw new KeyNotFoundException("Processamento de consolidação não encontrado.");
}

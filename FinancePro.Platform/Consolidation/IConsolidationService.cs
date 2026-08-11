namespace FinancePro.Platform.Consolidation;

public interface IConsolidationService
{
    Task<IReadOnlyList<ConsolidationCompany>> ListCompaniesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ConsolidationGroup>> ListGroupsAsync(CancellationToken cancellationToken = default);
    Task<int> SaveGroupAsync(SaveConsolidationGroupRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ConsolidationRun>> ListRunsAsync(int groupId, CancellationToken cancellationToken = default);
    Task<int> CreateRunAsync(CreateConsolidationRunRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ConsolidatedTrialBalanceRow>> GetTrialBalanceAsync(int runId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<IntercompanyDifference>> GetIntercompanyDifferencesAsync(int runId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ConsolidationElimination>> ListEliminationsAsync(int runId, CancellationToken cancellationToken = default);
    Task<int> SaveEliminationAsync(SaveConsolidationEliminationRequest request, CancellationToken cancellationToken = default);
    Task DeleteEliminationAsync(int runId, int eliminationId, CancellationToken cancellationToken = default);
    Task<ConsolidationValidationResult> ValidateAsync(int runId, int userId, string userName, CancellationToken cancellationToken = default);
    Task ConsolidateAsync(int runId, int userId, string userName, CancellationToken cancellationToken = default);
    Task CloseAsync(int runId, int userId, string userName, CancellationToken cancellationToken = default);
    Task<ConsolidationSummary> GetSummaryAsync(int runId, CancellationToken cancellationToken = default);
}

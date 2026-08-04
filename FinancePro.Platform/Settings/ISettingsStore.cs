namespace FinancePro.Platform.Settings;

public interface ISettingsStore
{
    Task<SystemSetting?> FindAsync(int companyId, string category, string key, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SystemSetting>> ListCategoryAsync(int companyId, string category, CancellationToken cancellationToken = default);
    Task UpsertAsync(int companyId, string category, string key, string? value, string dataType, string? description, int? updatedBy, CancellationToken cancellationToken = default);
}

namespace FinancePro.Platform.Settings;

public interface ISettingsService
{
    Task<T?> GetAsync<T>(int companyId, string category, string key, T? defaultValue = default, CancellationToken cancellationToken = default);
    Task SetAsync<T>(int companyId, string category, string key, T value, string? description = null, int? updatedBy = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SystemSetting>> GetCategoryAsync(int companyId, string category, CancellationToken cancellationToken = default);
}

using FinancePro.Platform.Settings;
using Xunit;

namespace FinancePro.Application.Tests.Platform;

public sealed class SettingsServiceTests
{
    [Fact]
    public async Task SetAndGetAsync_ShouldPreserveTypedValue()
    {
        var store = new MemorySettingsStore();
        var service = new SettingsService(store);

        await service.SetAsync(1, "Treasury", "AllowNegativeCash", true);
        var value = await service.GetAsync(1, "Treasury", "AllowNegativeCash", false);

        Assert.True(value);
    }

    private sealed class MemorySettingsStore : ISettingsStore
    {
        private readonly Dictionary<string, SystemSetting> _values = new();
        private static string Id(int companyId, string category, string key) => $"{companyId}:{category}:{key}";

        public Task<SystemSetting?> FindAsync(int companyId, string category, string key, CancellationToken cancellationToken = default)
        {
            _values.TryGetValue(Id(companyId, category, key), out var value);
            return Task.FromResult(value);
        }

        public Task<IReadOnlyList<SystemSetting>> ListCategoryAsync(int companyId, string category, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<SystemSetting>>(_values.Values.Where(x => x.CompanyId == companyId && x.Category == category).ToList());

        public Task UpsertAsync(int companyId, string category, string key, string? value, string dataType, string? description, int? updatedBy, CancellationToken cancellationToken = default)
        {
            _values[Id(companyId, category, key)] = new SystemSetting(1, companyId, category, key, value, dataType, description, true, DateTime.UtcNow, updatedBy);
            return Task.CompletedTask;
        }
    }
}

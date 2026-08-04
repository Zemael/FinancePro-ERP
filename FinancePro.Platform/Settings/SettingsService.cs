using System.Globalization;
using System.Text.Json;

namespace FinancePro.Platform.Settings;

public sealed class SettingsService : ISettingsService
{
    private readonly ISettingsStore _store;

    public SettingsService(ISettingsStore store) => _store = store;

    public async Task<T?> GetAsync<T>(int companyId, string category, string key, T? defaultValue = default, CancellationToken cancellationToken = default)
    {
        Validate(companyId, category, key);
        var setting = await _store.FindAsync(companyId, Normalize(category), Normalize(key), cancellationToken);
        if (setting?.Value is null) return defaultValue;

        try
        {
            if (typeof(T) == typeof(string)) return (T)(object)setting.Value;
            if (typeof(T) == typeof(bool) && bool.TryParse(setting.Value, out var boolean)) return (T)(object)boolean;
            if (typeof(T) == typeof(int) && int.TryParse(setting.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var integer)) return (T)(object)integer;
            if (typeof(T) == typeof(decimal) && decimal.TryParse(setting.Value, NumberStyles.Number, CultureInfo.InvariantCulture, out var number)) return (T)(object)number;
            return JsonSerializer.Deserialize<T>(setting.Value) ?? defaultValue;
        }
        catch (JsonException)
        {
            return defaultValue;
        }
    }

    public Task SetAsync<T>(int companyId, string category, string key, T value, string? description = null, int? updatedBy = null, CancellationToken cancellationToken = default)
    {
        Validate(companyId, category, key);
        var serialized = Serialize(value);
        var dataType = typeof(T).Name;
        return _store.UpsertAsync(companyId, Normalize(category), Normalize(key), serialized, dataType, description, updatedBy, cancellationToken);
    }

    public Task<IReadOnlyList<SystemSetting>> GetCategoryAsync(int companyId, string category, CancellationToken cancellationToken = default)
    {
        if (companyId <= 0) throw new ArgumentOutOfRangeException(nameof(companyId));
        if (string.IsNullOrWhiteSpace(category)) throw new ArgumentException("Categoria obrigatória.", nameof(category));
        return _store.ListCategoryAsync(companyId, Normalize(category), cancellationToken);
    }

    private static string Serialize<T>(T value) => value switch
    {
        null => string.Empty,
        string text => text,
        bool boolean => boolean.ToString(CultureInfo.InvariantCulture),
        IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
        _ => JsonSerializer.Serialize(value)
    };

    private static string Normalize(string value) => value.Trim().ToUpperInvariant();

    private static void Validate(int companyId, string category, string key)
    {
        if (companyId <= 0) throw new ArgumentOutOfRangeException(nameof(companyId));
        if (string.IsNullOrWhiteSpace(category)) throw new ArgumentException("Categoria obrigatória.", nameof(category));
        if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("Chave obrigatória.", nameof(key));
    }
}

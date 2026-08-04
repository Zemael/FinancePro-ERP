namespace FinancePro.Platform.Settings;

public sealed record SystemSetting(
    int Id,
    int CompanyId,
    string Category,
    string Key,
    string? Value,
    string DataType,
    string? Description,
    bool IsEditable,
    DateTime UpdatedAt,
    int? UpdatedBy);

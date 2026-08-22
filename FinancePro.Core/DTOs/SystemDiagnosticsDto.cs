namespace FinancePro.Core.DTOs;

public sealed class SystemDiagnosticsDto
{
    public string SqlServerVersion { get; set; } = string.Empty;
    public string DatabaseState { get; set; } = string.Empty;
    public string RecoveryModel { get; set; } = string.Empty;
    public int CompatibilityLevel { get; set; }
    public string IntegrityStatus { get; set; } = string.Empty;
    public DateTime CheckedAt { get; set; }
}

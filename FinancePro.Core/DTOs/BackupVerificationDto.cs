namespace FinancePro.Core.DTOs;

public sealed class BackupVerificationDto
{
    public DateTime CheckedAt { get; set; }
    public DateTime BackupCompletedAt { get; set; }
    public string BackupPath { get; set; } = string.Empty;
    public bool IsValid { get; set; }
    public string Status { get; set; } = string.Empty;
}

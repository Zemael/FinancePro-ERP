namespace FinancePro.Core.DTOs;

public sealed class DatabaseBackupDto
{
    public string DatabaseName { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public decimal SizeMb { get; set; }
    public int DurationSeconds { get; set; }
    public string BackupType { get; set; } = string.Empty;
    public bool IsCopyOnly { get; set; }
    public string Destination { get; set; } = string.Empty;
    public string Status => CompletedAt.HasValue ? "Concluído" : "Incompleto";
    public override string ToString() => $"{BackupType} · {StartedAt:dd/MM/yyyy HH:mm}";
}

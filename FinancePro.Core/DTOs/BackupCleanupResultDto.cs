namespace FinancePro.Core.DTOs;

public sealed class BackupCleanupResultDto
{
    public int Preserved { get; set; }
    public int Deleted { get; set; }
    public long ReleasedBytes { get; set; }
    public decimal ReleasedMb => Math.Round(ReleasedBytes / 1048576m, 2);
}

namespace FinancePro.Core.DTOs;

public sealed class SystemHealthDto
{
    public string DatabaseName { get; set; } = string.Empty;
    public string DataSource { get; set; } = string.Empty;
    public bool Connected { get; set; }
    public decimal DatabaseSizeMb { get; set; }
    public DateTime? LastBackupAt { get; set; }
    public int Empresas { get; set; }
    public int UtilizadoresAtivos { get; set; }
    public int LogsAuditoria { get; set; }
}

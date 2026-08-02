namespace FinancePro.Core.DTOs;

public class ConfiguracaoInicialDto
{
    public string EmpresaNome { get; set; } = string.Empty;
    public string Moeda { get; set; } = "FCFA";
    public string AdminNome { get; set; } = string.Empty;
    public string AdminEmail { get; set; } = string.Empty;
    public string AdminPassword { get; set; } = string.Empty;
}

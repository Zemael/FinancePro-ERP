namespace FinancePro.Core.DTOs;

public sealed class UtilizadorDto
{
    public int Id { get; set; }
    public string NomeCompleto { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? NovaPassword { get; set; }
    public int PerfilId { get; set; }
    public string PerfilNome { get; set; } = string.Empty;
    public int EmpresaId { get; set; }
    public string EmpresaNome { get; set; } = string.Empty;
    public DateTime? UltimoLogin { get; set; }
    public bool Ativo { get; set; } = true;
}

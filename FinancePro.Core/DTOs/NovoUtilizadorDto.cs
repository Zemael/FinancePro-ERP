namespace FinancePro.Core.DTOs;

public class NovoUtilizadorDto
{
    public string NomeCompleto { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public int PerfilId { get; set; }
    public int EmpresaId { get; set; }
}

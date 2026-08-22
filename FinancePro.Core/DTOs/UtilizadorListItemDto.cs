namespace FinancePro.Core.DTOs;

public class UtilizadorListItemDto
{
    public int Id { get; set; }
    public string NomeCompleto { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PerfilNome { get; set; } = string.Empty;
    public bool Ativo { get; set; }
    public override string ToString() => NomeCompleto;
}

namespace FinancePro.Core.DTOs;

public class EmpresaListItemDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? NIF { get; set; }
    public string? Telefone { get; set; }
    public string? Email { get; set; }
    public string Moeda { get; set; } = "FCFA";
    public bool Ativo { get; set; }

    /// <summary>
    /// Garante que controlos WPF exibem o nome da empresa quando não existe
    /// um DisplayMemberPath explícito, em vez do nome completo do tipo DTO.
    /// </summary>
    public override string ToString() => Nome;
}

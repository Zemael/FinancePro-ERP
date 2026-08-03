namespace FinancePro.Core.DTOs;

public sealed class FornecedorDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? NIF { get; set; }
    public string? Telefone { get; set; }
    public string? Email { get; set; }
    public string? Morada { get; set; }
    public bool Ativo { get; set; }
}

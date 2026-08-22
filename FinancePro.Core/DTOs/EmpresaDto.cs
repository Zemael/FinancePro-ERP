namespace FinancePro.Core.DTOs;

public class EmpresaDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? NIF { get; set; }
    public string? Morada { get; set; }
    public string? Telefone { get; set; }
    public string? Email { get; set; }
    public string Moeda { get; set; } = "FCFA";
    public byte[]? Logotipo { get; set; }
    public string? BancoNome { get; set; }
    public string? BancoConta { get; set; }
    public string? BancoIban { get; set; }
    public string? BancoTitular { get; set; }
    public override string ToString() => Nome;
}

namespace FinancePro.Core.DTOs;

public class BancoListItemDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Sigla { get; set; }
    public string? CodigoSwift { get; set; }
    public string? Endereco { get; set; }
    public string? Contacto { get; set; }
    public override string ToString() => Nome;
}

namespace FinancePro.Core.DTOs;

public class NovoBancoDto
{
    public string Nome { get; set; } = string.Empty;
    public string? Sigla { get; set; }
    public string? CodigoSwift { get; set; }
    public string? Endereco { get; set; }
    public string? Contacto { get; set; }
}

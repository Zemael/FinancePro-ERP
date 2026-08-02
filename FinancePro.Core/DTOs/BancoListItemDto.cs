namespace FinancePro.Core.DTOs;

public class BancoListItemDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? CodigoSwift { get; set; }
}

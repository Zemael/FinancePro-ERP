namespace FinancePro.Core.DTOs;

public class CaixaListItemDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public decimal SaldoInicial { get; set; }
    public decimal? SaldoMinimo { get; set; }
    public bool PermiteSaldoNegativo { get; set; }
    public bool Ativo { get; set; }
    public override string ToString() => Nome;
}

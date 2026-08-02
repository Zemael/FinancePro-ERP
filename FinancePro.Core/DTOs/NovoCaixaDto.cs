namespace FinancePro.Core.DTOs;

public class NovoCaixaDto
{
    public string Nome { get; set; } = string.Empty;
    public decimal SaldoInicial { get; set; }
    public decimal? SaldoMinimo { get; set; }
    public bool PermiteSaldoNegativo { get; set; }
    public int EmpresaId { get; set; }
}

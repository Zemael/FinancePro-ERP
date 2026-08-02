namespace FinancePro.Core.DTOs;

public sealed class AbrirCaixaDto
{
    public int CaixaId { get; set; }
    public int EmpresaId { get; set; }
    public int UtilizadorId { get; set; }
    public decimal SaldoInicial { get; set; }
    public string? Observacao { get; set; }
}

namespace FinancePro.Core.DTOs;

public sealed class FecharCaixaDto
{
    public int SessaoCaixaId { get; set; }
    public decimal SaldoContado { get; set; }
    public string? Observacao { get; set; }
}

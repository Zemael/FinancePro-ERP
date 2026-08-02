namespace FinancePro.Core.DTOs;

public sealed class SessaoCaixaDto
{
    public int Id { get; set; }
    public int CaixaId { get; set; }
    public string CaixaNome { get; set; } = string.Empty;
    public string OperadorNome { get; set; } = string.Empty;
    public DateTime DataAbertura { get; set; }
    public decimal SaldoInicial { get; set; }
    public decimal SaldoAtual { get; set; }
    public bool Aberta { get; set; }
}

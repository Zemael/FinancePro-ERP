using FinancePro.Core.Enums;

namespace FinancePro.Core.DTOs;

public sealed class LancamentoContabilDto
{
    public int Id { get; set; }
    public string Numero { get; set; } = string.Empty;
    public DateTime DataLancamento { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public EstadoLancamentoContabil Estado { get; set; }
    public decimal TotalDebito { get; set; }
    public decimal TotalCredito { get; set; }
}

public sealed class LancamentoContabilLinhaDto
{
    public int PlanoContasId { get; set; }
    public string Conta { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public decimal Debito { get; set; }
    public decimal Credito { get; set; }
    public string? CentroCusto { get; set; }
}

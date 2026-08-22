namespace FinancePro.Core.DTOs;

/// <summary>Um ponto (mês) da série de fluxo de caixa do Dashboard.</summary>
public class SerieMensalDto
{
    public string Mes { get; set; } = string.Empty;
    public decimal SaldoAcumulado { get; set; }
    public decimal Entradas { get; set; }
    public decimal Saidas { get; set; }
}

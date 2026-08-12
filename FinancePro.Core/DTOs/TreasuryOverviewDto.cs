namespace FinancePro.Core.DTOs;

public sealed class TreasuryOverviewDto
{
    public decimal SaldoDisponivel { get; set; }
    public decimal TotalCaixa { get; set; }
    public decimal TotalBancos { get; set; }
    public decimal AReceberPendente { get; set; }
    public decimal APagarPendente { get; set; }
    public decimal AReceberAtrasado { get; set; }
    public decimal APagarAtrasado { get; set; }
    public decimal ReceberProximos7Dias { get; set; }
    public decimal PagarProximos7Dias { get; set; }
    public decimal Previsao7Dias { get; set; }
    public decimal Previsao15Dias { get; set; }
    public decimal Previsao30Dias { get; set; }
    public decimal Previsao60Dias { get; set; }
    public decimal InadimplenciaPercentual { get; set; }
    public string RiscoLiquidez { get; set; } = "Baixo";
    public int TitulosReceberAtrasados { get; set; }
    public int TitulosPagarAtrasados { get; set; }
}

public sealed class TreasuryForecastItemDto
{
    public DateTime Data { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public string Entidade { get; set; } = string.Empty;
    public decimal Entrada { get; set; }
    public decimal Saida { get; set; }
    public bool Atrasado { get; set; }
}

public sealed class TreasuryAgingDto
{
    public string Faixa { get; set; } = string.Empty;
    public decimal AReceber { get; set; }
    public decimal APagar { get; set; }
    public decimal Liquido => AReceber - APagar;
}

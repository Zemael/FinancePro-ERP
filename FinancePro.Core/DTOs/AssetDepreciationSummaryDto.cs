namespace FinancePro.Core.DTOs;

public sealed class AssetDepreciationSummaryDto
{
    public int TotalAtivos { get; set; }
    public decimal ValorAquisicaoTotal { get; set; }
    public decimal ValorResidualTotal { get; set; }
    public decimal DepreciacaoAcumuladaTotal { get; set; }
    public decimal ValorLiquidoTotal { get; set; }
    public decimal DepreciacaoMensalTotal { get; set; }
}

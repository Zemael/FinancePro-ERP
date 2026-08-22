namespace FinancePro.Core.DTOs;

public class ExecucaoMensalDto
{
    public int Mes { get; set; }
    public string MesNome { get; set; } = string.Empty;
    public decimal PrevistoReceitas { get; set; }
    public decimal RealizadoReceitas { get; set; }
    public decimal PrevistoDespesas { get; set; }
    public decimal RealizadoDespesas { get; set; }
    public decimal ComprometidoDespesas { get; set; }
    public decimal SaldoDisponivelDespesas => PrevistoDespesas - RealizadoDespesas - ComprometidoDespesas;
    public decimal ExecucaoDespesasPercentual => PrevistoDespesas > 0 ? Math.Round(RealizadoDespesas / PrevistoDespesas * 100m, 1) : 0m;
    public decimal UtilizacaoDespesasPercentual => PrevistoDespesas > 0 ? Math.Round((RealizadoDespesas + ComprometidoDespesas) / PrevistoDespesas * 100m, 1) : 0m;
    public string SituacaoOrcamental => SaldoDisponivelDespesas < 0 ? "Excedido" : UtilizacaoDespesasPercentual >= 90 ? "Atenção" : "Dentro do orçamento";
    public override string ToString() => MesNome;
}

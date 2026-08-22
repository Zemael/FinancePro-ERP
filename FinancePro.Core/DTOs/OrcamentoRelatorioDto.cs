namespace FinancePro.Core.DTOs;

public class OrcamentoRelatorioDto
{
    public decimal TotalPrevistoReceitas { get; set; }
    public decimal TotalRealizadoReceitas { get; set; }
    public decimal TotalPrevistoDespesas { get; set; }
    public decimal TotalRealizadoDespesas { get; set; }
    public decimal TotalComprometidoDespesas { get; set; }
    public decimal SaldoDisponivelDespesas => TotalPrevistoDespesas - TotalRealizadoDespesas - TotalComprometidoDespesas;
    public decimal ExecucaoDespesasPercentual => TotalPrevistoDespesas > 0 ? Math.Round(TotalRealizadoDespesas / TotalPrevistoDespesas * 100m, 1) : 0m;
    public decimal UtilizacaoDespesasPercentual => TotalPrevistoDespesas > 0 ? Math.Round((TotalRealizadoDespesas + TotalComprometidoDespesas) / TotalPrevistoDespesas * 100m, 1) : 0m;
    public decimal ResultadoPrevisto => TotalPrevistoReceitas - TotalPrevistoDespesas;
    public decimal ResultadoRealizado => TotalRealizadoReceitas - TotalRealizadoDespesas;
    public string AlertaOrcamental => SaldoDisponivelDespesas < 0 ? "Orçamento excedido" : UtilizacaoDespesasPercentual >= 90 ? "Execução próxima do limite" : "Execução controlada";
}

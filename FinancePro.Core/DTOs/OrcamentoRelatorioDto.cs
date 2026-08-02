namespace FinancePro.Core.DTOs;

public class OrcamentoRelatorioDto
{
    public decimal TotalPrevistoReceitas { get; set; }
    public decimal TotalRealizadoReceitas { get; set; }
    public decimal TotalPrevistoDespesas { get; set; }
    public decimal TotalRealizadoDespesas { get; set; }
    public decimal ResultadoPrevisto => TotalPrevistoReceitas - TotalPrevistoDespesas;
    public decimal ResultadoRealizado => TotalRealizadoReceitas - TotalRealizadoDespesas;
}

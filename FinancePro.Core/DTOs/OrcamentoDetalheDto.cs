namespace FinancePro.Core.DTOs;

public class OrcamentoDetalheDto
{
    public int Id { get; set; }
    public string PlanoContasNome { get; set; } = string.Empty;
    public string? CentroCusto { get; set; }
    public string? Departamento { get; set; }
    public int Mes { get; set; }
    public string MesNome { get; set; } = string.Empty;
    public decimal ValorPrevisto { get; set; }
    public decimal ValorRealizado { get; set; }
    public decimal Desvio => ValorRealizado - ValorPrevisto;
}

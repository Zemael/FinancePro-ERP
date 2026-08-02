using FinancePro.Core.Enums;

namespace FinancePro.Core.DTOs;

public class NovoOrcamentoDetalheDto
{
    public int OrcamentoId { get; set; }
    public int PlanoContasId { get; set; }
    public string? CentroCusto { get; set; }
    public string? Departamento { get; set; }
    public TipoCategoria Tipo { get; set; }
    public int Mes { get; set; }
    public decimal ValorPrevisto { get; set; }
}

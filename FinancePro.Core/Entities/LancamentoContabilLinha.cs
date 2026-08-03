namespace FinancePro.Core.Entities;

public class LancamentoContabilLinha : EntityBase
{
    public int LancamentoContabilId { get; set; }
    public LancamentoContabil LancamentoContabil { get; set; } = null!;
    public int PlanoContasId { get; set; }
    public PlanoContas PlanoContas { get; set; } = null!;
    public string? Descricao { get; set; }
    public decimal Debito { get; set; }
    public decimal Credito { get; set; }
    public string? CentroCusto { get; set; }
}

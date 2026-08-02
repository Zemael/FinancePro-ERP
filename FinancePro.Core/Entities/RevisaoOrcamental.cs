using FinancePro.Core.Enums;

namespace FinancePro.Core.Entities;

public class RevisaoOrcamental : EntityBase
{
    public int OrcamentoId { get; set; }
    public Orcamento Orcamento { get; set; } = null!;

    public int Versao { get; set; }
    public DateTime Data { get; set; }
    public string Motivo { get; set; } = string.Empty;
    public string Responsavel { get; set; } = string.Empty;
    public EstadoRevisao Estado { get; set; } = EstadoRevisao.Pendente;
}

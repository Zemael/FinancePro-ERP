using FinancePro.Core.Enums;

namespace FinancePro.Core.Entities;

/// <summary>Cabeçalho de um orçamento (ex.: "Orçamento 2026").</summary>
public class Orcamento : EntityBase
{
    public int Ano { get; set; }
    public string Nome { get; set; } = string.Empty;
    public DateTime DataInicio { get; set; }
    public DateTime DataFim { get; set; }
    public string Moeda { get; set; } = "FCFA";
    public EstadoOrcamento Estado { get; set; } = EstadoOrcamento.Rascunho;
    public string? Observacoes { get; set; }

    public string? ElaboradoPor { get; set; }
    public string? RevistoPor { get; set; }
    public string? AprovadoPor { get; set; }
    public DateTime? DataAprovacao { get; set; }

    public int EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = null!;

    public ICollection<OrcamentoDetalhe> Detalhes { get; set; } = new List<OrcamentoDetalhe>();
    public ICollection<RevisaoOrcamental> Revisoes { get; set; } = new List<RevisaoOrcamental>();
}

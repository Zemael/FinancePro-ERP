using FinancePro.Core.Enums;

namespace FinancePro.Core.Entities;

/// <summary>
/// Uma linha do orçamento: uma conta do Plano de Contas, num mês, com o
/// valor previsto. O valor realizado NÃO é guardado aqui — é calculado a
/// partir dos Movimentos já lançados (ligados à mesma Categoria/PlanoContas),
/// para nunca ficar desalinhado com a tesouraria real.
///
/// Centro de Custo e Departamento ainda não são cadastros próprios neste
/// projeto — ficam como texto livre até esses módulos existirem.
/// </summary>
public class OrcamentoDetalhe : EntityBase
{
    public int OrcamentoId { get; set; }
    public Orcamento Orcamento { get; set; } = null!;

    public int PlanoContasId { get; set; }
    public PlanoContas PlanoContas { get; set; } = null!;

    public string? CentroCusto { get; set; }
    public string? Departamento { get; set; }

    public TipoCategoria Tipo { get; set; }
    public int Mes { get; set; }
    public decimal ValorPrevisto { get; set; }
}

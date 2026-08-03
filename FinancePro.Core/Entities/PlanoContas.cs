namespace FinancePro.Core.Entities;

using FinancePro.Core.Enums;

/// <summary>Plano de contas hierárquico da empresa.</summary>
public class PlanoContas : EntityBase
{
    public string Codigo { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public TipoConta Tipo { get; set; }
    public NaturezaContabil Natureza { get; set; } = NaturezaContabil.Devedora;
    public bool AceitaLancamentos { get; set; } = true;
    public bool CentroCustoObrigatorio { get; set; }

    public int? ContaPaiId { get; set; }
    public PlanoContas? ContaPai { get; set; }
    public ICollection<PlanoContas> SubContas { get; set; } = new List<PlanoContas>();

    public int EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = null!;

    public ICollection<Categoria> Categorias { get; set; } = new List<Categoria>();
    public ICollection<LancamentoContabilLinha> LinhasLancamento { get; set; } = new List<LancamentoContabilLinha>();
}

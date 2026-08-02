namespace FinancePro.Core.Entities;

using FinancePro.Core.Enums;

/// <summary>Plano de contas da empresa, com hierarquia (conta-mãe / sub-contas).</summary>
public class PlanoContas : EntityBase
{
    public string Codigo { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public TipoConta Tipo { get; set; }

    public int? ContaPaiId { get; set; }
    public PlanoContas? ContaPai { get; set; }
    public ICollection<PlanoContas> SubContas { get; set; } = new List<PlanoContas>();

    public int EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = null!;

    public ICollection<Categoria> Categorias { get; set; } = new List<Categoria>();
}

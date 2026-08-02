namespace FinancePro.Core.Entities;

using FinancePro.Core.Enums;

/// <summary>Categoria usada para classificar receitas e despesas.</summary>
public class Categoria : EntityBase
{
    public string Nome { get; set; } = string.Empty;
    public TipoCategoria Tipo { get; set; }

    public int? PlanoContasId { get; set; }
    public PlanoContas? PlanoContas { get; set; }

    public int EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = null!;
}

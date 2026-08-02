namespace FinancePro.Core.Entities;

/// <summary>Banco (entidade financeira), independente da empresa — catálogo partilhado.</summary>
public class Banco : EntityBase
{
    public string Nome { get; set; } = string.Empty;
    public string? CodigoSwift { get; set; }

    public ICollection<ContaBancaria> ContasBancarias { get; set; } = new List<ContaBancaria>();
}

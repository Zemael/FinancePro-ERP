namespace FinancePro.Core.Entities;

/// <summary>Período financeiro utilizado para registos, orçamento e relatórios.</summary>
public sealed class ExercicioFinanceiro : EntityBase
{
    public int EmpresaId { get; set; }
    public int Ano { get; set; }
    public DateTime DataInicio { get; set; }
    public DateTime DataFim { get; set; }
    public bool Padrao { get; set; }
    public bool Encerrado { get; set; }

    public Empresa Empresa { get; set; } = null!;
}

namespace FinancePro.Core.Entities;

/// <summary>Caixa de tesouraria (dinheiro físico) de uma empresa.</summary>
public class Caixa : EntityBase
{
    public string Nome { get; set; } = string.Empty;
    public decimal SaldoInicial { get; set; }
    public decimal? SaldoMinimo { get; set; }
    public bool PermiteSaldoNegativo { get; set; }

    public int EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = null!;
}

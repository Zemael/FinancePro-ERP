using FinancePro.Core.Enums;

namespace FinancePro.Core.Entities;

/// <summary>Período operacional de um caixa, da abertura ao fecho.</summary>
public sealed class SessaoCaixa : EntityBase
{
    public int CaixaId { get; set; }
    public Caixa Caixa { get; set; } = null!;

    public int EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = null!;

    public int ExercicioFinanceiroId { get; set; }
    public ExercicioFinanceiro ExercicioFinanceiro { get; set; } = null!;

    public int UtilizadorId { get; set; }
    public Utilizador Utilizador { get; set; } = null!;

    public DateTime DataAbertura { get; set; } = DateTime.UtcNow;
    public decimal SaldoInicial { get; set; }
    public string? ObservacaoAbertura { get; set; }

    public DateTime? DataFecho { get; set; }
    public decimal? SaldoContado { get; set; }
    public decimal? SaldoCalculado { get; set; }
    public decimal? Diferenca { get; set; }
    public string? ObservacaoFecho { get; set; }

    public EstadoSessaoCaixa Estado { get; set; } = EstadoSessaoCaixa.Aberta;
}

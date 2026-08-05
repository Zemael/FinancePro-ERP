namespace FinancePro.Core.DTOs;

/// <summary>Resumo completo para o Dashboard: cabeçalho, cartões por área,
/// indicadores, saldos por origem, pendências e alertas.</summary>
public class DashboardResumoDto
{
    // Cabeçalho
    public string EmpresaNome { get; set; } = string.Empty;
    public int Exercicio { get; set; }

    // Tesouraria (dados reais)
    public decimal SaldoCaixa { get; set; }
    public decimal SaldoBancario { get; set; }
    public decimal SaldoTesouraria => SaldoCaixa + SaldoBancario;

    // Receitas / Despesas (dados reais — vêm da Tesouraria/Receitas já construídas)
    public decimal TotalReceitas { get; set; }
    public decimal TotalDespesas { get; set; }
    public decimal Resultado => TotalReceitas - TotalDespesas;
    public decimal MargemPercentual => TotalReceitas > 0 ? Math.Round(Resultado / TotalReceitas * 100, 1) : 0;

    // Orçamento / Património — módulos ainda não construídos, ficam a placeholder
    public bool OrcamentoDisponivel => false;
    public bool PatrimonioDisponivel => false;

    // Conformidade fiscal
    public int ObrigacoesFiscaisAtrasadas { get; set; }
    public int ObrigacoesFiscaisProximas { get; set; }
    public int ObrigacoesFiscaisCumpridas { get; set; }
    public decimal TaxaConformidadeFiscal { get; set; }

    public List<MovimentoRecenteDto> MovimentosRecentes { get; set; } = new();
    public List<SaldoOrigemDto> SaldosPorOrigem { get; set; } = new();
    public List<ContaReceberListItemDto> Pendencias { get; set; } = new();
    public List<AlertaDto> Alertas { get; set; } = new();
}

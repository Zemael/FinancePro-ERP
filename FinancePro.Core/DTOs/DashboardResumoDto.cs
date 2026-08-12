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

    // Orçamento (dados reais do orçamento aprovado/ativo do exercício)
    public decimal OrcamentoPrevistoDespesas { get; set; }
    public decimal OrcamentoRealizadoDespesas { get; set; }
    public decimal ExecucaoOrcamentalPercentual => OrcamentoPrevistoDespesas > 0
        ? Math.Round(OrcamentoRealizadoDespesas / OrcamentoPrevistoDespesas * 100, 1) : 0;

    // Património (dados reais)
    public int BensAtivos { get; set; }
    public int BensEmManutencao { get; set; }
    public decimal ValorPatrimonio { get; set; }

    // Workflow (tarefas do utilizador atual)
    public int WorkflowPendentes { get; set; }
    public int WorkflowAtrasados { get; set; }
    public int WorkflowUrgentes { get; set; }

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

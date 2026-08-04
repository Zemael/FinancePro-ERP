namespace FinancePro.Core.DTOs;

/// <summary>Resumo executivo consolidado do FinancePro.</summary>
public class DashboardResumoDto
{
    public string EmpresaNome { get; set; } = string.Empty;
    public int Exercicio { get; set; }

    public decimal SaldoCaixa { get; set; }
    public decimal SaldoBancario { get; set; }
    public decimal SaldoTesouraria => SaldoCaixa + SaldoBancario;

    public decimal TotalReceitas { get; set; }
    public decimal TotalDespesas { get; set; }
    public decimal Resultado => TotalReceitas - TotalDespesas;
    public decimal MargemPercentual => TotalReceitas > 0 ? Math.Round(Resultado / TotalReceitas * 100, 1) : 0;

    public decimal TotalAReceber { get; set; }
    public decimal TotalAPagar { get; set; }
    public int ComprasPendentes { get; set; }
    public decimal ValorPatrimonio { get; set; }
    public decimal TotalOrcamento { get; set; }
    public decimal ExecucaoOrcamental { get; set; }
    public int TotalAlertas { get; set; }

    public List<MovimentoRecenteDto> MovimentosRecentes { get; set; } = new();
    public List<SaldoOrigemDto> SaldosPorOrigem { get; set; } = new();
    public List<ContaReceberListItemDto> Pendencias { get; set; } = new();
    public List<AlertaDto> Alertas { get; set; } = new();
    public List<AtividadeRecenteDto> AtividadesRecentes { get; set; } = new();
    public List<FluxoCaixaMensalDto> FluxoMensal { get; set; } = new();
}

namespace FinancePro.Core.DTOs;

public class ContaPagarListItemDto
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public decimal ValorLiquidado { get; set; }
    public decimal SaldoAberto => Math.Max(0, Valor - ValorLiquidado);
    public int DiasAtraso => SaldoAberto > 0 && DataVencimento.Date < DateTime.Today ? (DateTime.Today - DataVencimento.Date).Days : 0;
    public DateTime DataEmissao { get; set; }
    public DateTime DataVencimento { get; set; }
    public DateTime? DataPagamento { get; set; }
    public string? FornecedorNome { get; set; }
    public string? CategoriaNome { get; set; }
    public string? FormaPagamento { get; set; }
    public string? CentroCusto { get; set; }

    /// <summary>Pendente | Atrasado | Paga | Cancelada.</summary>
    public string EstadoExibicao { get; set; } = string.Empty;

    public bool PodePagar { get; set; }
    public bool PodeCancelar { get; set; }
}

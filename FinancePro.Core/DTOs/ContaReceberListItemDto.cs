namespace FinancePro.Core.DTOs;

public class ContaReceberListItemDto
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
    public DateTime? DataRecebimento { get; set; }
    public string? ClienteNome { get; set; }
    public string? CategoriaNome { get; set; }
    public string? FormaPagamento { get; set; }
    public string? CentroCusto { get; set; }

    /// <summary>Pendente | Atrasado | Recebido | Cancelado — "Atrasado" é calculado
    /// (Pendente + vencimento já passado), não é um valor guardado.</summary>
    public string EstadoExibicao { get; set; } = string.Empty;

    public bool PodeReceber { get; set; }
    public bool PodeCancelar { get; set; }
}

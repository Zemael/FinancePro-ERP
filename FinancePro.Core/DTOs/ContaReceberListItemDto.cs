namespace FinancePro.Core.DTOs;

public class ContaReceberListItemDto
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public decimal ValorLiquidado { get; set; }
    public string ComercialEstado { get; set; } = "Faturada";
    public string? NumeroProposta { get; set; }
    public string? NumeroFatura { get; set; }
    public DateTime? DataAprovacao { get; set; }
    public DateTime? DataFaturacao { get; set; }
    public bool PodeAprovar => ComercialEstado == "Proposta";
    public bool PodeFaturar => ComercialEstado == "Aprovada";
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

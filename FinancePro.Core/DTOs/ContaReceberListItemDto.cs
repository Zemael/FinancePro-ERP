namespace FinancePro.Core.DTOs;

public class ContaReceberListItemDto
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public decimal Valor { get; set; }
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

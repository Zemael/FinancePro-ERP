namespace FinancePro.Core.DTOs;

public class ContaPagarListItemDto
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public decimal Valor { get; set; }
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

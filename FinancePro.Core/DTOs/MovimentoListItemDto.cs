namespace FinancePro.Core.DTOs;

public class MovimentoListItemDto
{
    public int Id { get; set; }
    public DateTime Data { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public string TipoOperacao { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public bool Conciliado { get; set; }
    public decimal Valor { get; set; }
    public string Origem { get; set; } = string.Empty;
    public string? CategoriaNome { get; set; }
    public string? FormaPagamento { get; set; }
    public string? CentroCusto { get; set; }
    public override string ToString() => Descricao;
}

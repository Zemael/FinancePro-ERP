namespace FinancePro.Core.DTOs;

public class NovaContaReceberDto
{
    public string Descricao { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public DateTime DataEmissao { get; set; } = DateTime.Today;
    public DateTime DataVencimento { get; set; } = DateTime.Today;
    public string? FormaPagamento { get; set; }
    public string? CentroCusto { get; set; }
    public int? ClienteId { get; set; }
    public int? CategoriaId { get; set; }
    public int EmpresaId { get; set; }
}

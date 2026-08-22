namespace FinancePro.Core.DTOs;

public class NovaContaReceberDto
{
    public string Descricao { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public decimal DescontoGeral { get; set; }
    public decimal Frete { get; set; }
    public decimal OutrasDespesas { get; set; }
    public string? Observacoes { get; set; }
    public DateTime DataEmissao { get; set; } = DateTime.Today;
    public DateTime DataVencimento { get; set; } = DateTime.Today;
    public string? FormaPagamento { get; set; }
    public string? CentroCusto { get; set; }
    public int? ClienteId { get; set; }
    public int? CategoriaId { get; set; }
    public int EmpresaId { get; set; }
    public bool CriarComoProposta { get; set; } = true;
}

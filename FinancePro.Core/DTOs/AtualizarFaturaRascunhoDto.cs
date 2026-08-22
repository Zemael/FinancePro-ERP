namespace FinancePro.Core.DTOs;

public sealed class AtualizarFaturaRascunhoDto
{
    public int Id { get; set; }
    public int EmpresaId { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public DateTime DataEmissao { get; set; }
    public DateTime DataVencimento { get; set; }
    public int? ClienteId { get; set; }
    public int? CategoriaId { get; set; }
    public string? FormaPagamento { get; set; }
    public string? CentroCusto { get; set; }
    public decimal DescontoGeral { get; set; }
    public decimal Frete { get; set; }
    public decimal OutrasDespesas { get; set; }
    public string? Observacoes { get; set; }
}

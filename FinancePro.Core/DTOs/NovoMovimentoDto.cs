using FinancePro.Core.Enums;

namespace FinancePro.Core.DTOs;

public class NovoMovimentoDto
{
    public DateTime Data { get; set; } = DateTime.Today;
    public string Descricao { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public TipoCategoria Tipo { get; set; }
    public TipoOperacao TipoOperacao { get; set; } = TipoOperacao.Entrada;
    public string? FormaPagamento { get; set; }
    public string? CentroCusto { get; set; }
    public bool Conciliado { get; set; }
    public Guid? GrupoTransferenciaId { get; set; }
    public int? CategoriaId { get; set; }
    public int? CaixaId { get; set; }
    public int? ContaBancariaId { get; set; }
    public int? ClienteId { get; set; }
    public int EmpresaId { get; set; }
}

using FinancePro.Core.Enums;
namespace FinancePro.Core.Entities;
public class MovimentoStock : EntityBase
{
    public int ProdutoId { get; set; }
    public Produto Produto { get; set; } = null!;
    public DateTime Data { get; set; } = DateTime.Now;
    public TipoMovimentoStock Tipo { get; set; }
    public decimal Quantidade { get; set; }
    public decimal CustoUnitario { get; set; }
    public decimal SaldoApos { get; set; }
    public string? DocumentoReferencia { get; set; }
    public string? Observacao { get; set; }
    public int EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = null!;
}

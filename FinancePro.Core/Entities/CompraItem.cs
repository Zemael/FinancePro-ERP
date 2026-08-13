namespace FinancePro.Core.Entities;
public class CompraItem : EntityBase
{
    public int CompraId { get; set; }
    public Compra Compra { get; set; } = null!;
    public int ProdutoId { get; set; }
    public Produto Produto { get; set; } = null!;
    public decimal Quantidade { get; set; }
    public decimal PrecoUnitario { get; set; }
    public decimal DescontoPercentual { get; set; }
    public decimal IvaPercentual { get; set; }
    public decimal Subtotal => Math.Round(Quantidade * PrecoUnitario * (1 - DescontoPercentual / 100m), 2);
    public decimal ValorIva => Math.Round(Subtotal * IvaPercentual / 100m, 2);
    public decimal Total => Subtotal + ValorIva;
}

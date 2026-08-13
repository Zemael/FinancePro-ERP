namespace FinancePro.Core.Entities;
public class VendaItem : EntityBase
{
    public int ContaReceberId { get; set; }
    public ContaReceber ContaReceber { get; set; } = null!;
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

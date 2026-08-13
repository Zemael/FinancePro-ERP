using FinancePro.Core.Entities; using Xunit;
namespace FinancePro.Application.Tests.Documents;
public class DocumentLineCalculationTests { [Fact] public void CompraItem_Calcula_Desconto_Iva_Total(){ var i=new CompraItem{Quantidade=2,PrecoUnitario=100,DescontoPercentual=10,IvaPercentual=19}; Assert.Equal(180m,i.Subtotal); Assert.Equal(34.20m,i.ValorIva); Assert.Equal(214.20m,i.Total); } [Fact] public void VendaItem_Calcula_Total(){ var i=new VendaItem{Quantidade=3,PrecoUnitario=50,DescontoPercentual=0,IvaPercentual=0}; Assert.Equal(150m,i.Total); } }

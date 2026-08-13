namespace FinancePro.Core.DTOs;
public sealed class DocumentoItemDto
{
 public int Id {get;set;} public int ProdutoId {get;set;} public string ProdutoCodigo {get;set;}=""; public string ProdutoNome {get;set;}=""; public string Unidade {get;set;}="UN"; public decimal Quantidade {get;set;} public decimal PrecoUnitario {get;set;} public decimal DescontoPercentual {get;set;} public decimal IvaPercentual {get;set;} public decimal Subtotal {get;set;} public decimal ValorIva {get;set;} public decimal Total {get;set;}
}
public sealed class NovoDocumentoItemDto
{
 public int DocumentoId {get;set;} public int ProdutoId {get;set;} public decimal Quantidade {get;set;} public decimal PrecoUnitario {get;set;} public decimal DescontoPercentual {get;set;} public decimal IvaPercentual {get;set;}
}

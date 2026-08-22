using FinancePro.Core.Enums;
namespace FinancePro.Core.DTOs;
public sealed class ProdutoStockDto { public int Id {get;set;} public string Codigo {get;set;}=""; public string Nome {get;set;}=""; public string Categoria {get;set;}=""; public string Unidade {get;set;}="UN"; public decimal StockAtual {get;set;} public decimal StockMinimo {get;set;} public decimal CustoMedio {get;set;} public decimal PrecoVenda {get;set;} public bool ControlaStock {get;set;}=true; public string TipoItem => ControlaStock?"Produto":"Serviço"; public decimal ValorStock => ControlaStock?StockAtual*CustoMedio:0; public string Localizacao {get;set;}=""; public bool AbaixoMinimo => ControlaStock && StockAtual <= StockMinimo; public override string ToString()=>string.IsNullOrWhiteSpace(Codigo)?Nome:$"{Codigo} · {Nome}"; }
public sealed class NovoProdutoDto { public int EmpresaId {get;set;} public string Codigo {get;set;}=""; public string Nome {get;set;}=""; public string Categoria {get;set;}=""; public string Unidade {get;set;}="UN"; public decimal StockMinimo {get;set;} public decimal PrecoVenda {get;set;} public bool ControlaStock {get;set;}=true; public string? Localizacao {get;set;} }
public sealed class NovoMovimentoStockDto { public int EmpresaId {get;set;} public int ProdutoId {get;set;} public TipoMovimentoStock Tipo {get;set;} public decimal Quantidade {get;set;} public decimal CustoUnitario {get;set;} public string? DocumentoReferencia {get;set;} public string? Observacao {get;set;} }
public sealed class MovimentoStockDto { public DateTime Data {get;set;} public string Produto {get;set;}=""; public string Tipo {get;set;}=""; public decimal Quantidade {get;set;} public decimal CustoUnitario {get;set;} public decimal SaldoApos {get;set;} public string DocumentoReferencia {get;set;}=""; public override string ToString()=>Produto; }

public sealed class ProdutoRentabilidadeDto
{
    public int ProdutoId { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Produto { get; set; } = string.Empty;
    public decimal QuantidadeVendida { get; set; }
    public decimal ReceitaLiquida { get; set; }
    public decimal CustoEstimado { get; set; }
    public decimal MargemBruta => ReceitaLiquida - CustoEstimado;
    public decimal MargemPercentual => ReceitaLiquida == 0 ? 0 : Math.Round(MargemBruta / ReceitaLiquida * 100m, 2);
    public override string ToString() => string.IsNullOrWhiteSpace(Codigo) ? Produto : $"{Codigo} · {Produto}";
}
public sealed class ResumoCustosMargensDto
{
    public decimal ReceitaLiquida { get; set; }
    public decimal CustoProdutosVendidos { get; set; }
    public decimal MargemBruta => ReceitaLiquida - CustoProdutosVendidos;
    public decimal MargemPercentual => ReceitaLiquida == 0 ? 0 : Math.Round(MargemBruta / ReceitaLiquida * 100m, 2);
    public decimal ValorInventario { get; set; }
    public int ProdutosComMargemNegativa { get; set; }
}

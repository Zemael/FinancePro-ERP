namespace FinancePro.Core.Entities;
public class Produto : EntityBase
{
    public string Codigo { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public string Unidade { get; set; } = "UN";
    public decimal StockMinimo { get; set; }
    public decimal StockAtual { get; set; }
    public decimal CustoMedio { get; set; }
    public decimal PrecoVenda { get; set; }
    public bool ControlaStock { get; set; } = true;
    public string? Localizacao { get; set; }
    public int EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = null!;
}

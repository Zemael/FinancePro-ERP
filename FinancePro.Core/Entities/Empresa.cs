namespace FinancePro.Core.Entities;

/// <summary>Empresa que utiliza o sistema (raiz multi-empresa).</summary>
public class Empresa : EntityBase
{
    public string Nome { get; set; } = string.Empty;
    public string? NIF { get; set; }
    public string? Morada { get; set; }
    public string? Telefone { get; set; }
    public string? Email { get; set; }
    public string Moeda { get; set; } = "FCFA";
    public byte[]? Logotipo { get; set; }

    public ICollection<Utilizador> Utilizadores { get; set; } = new List<Utilizador>();
    public ICollection<ExercicioFinanceiro> ExerciciosFinanceiros { get; set; } = new List<ExercicioFinanceiro>();
    public ICollection<ContaBancaria> ContasBancarias { get; set; } = new List<ContaBancaria>();
    public ICollection<Caixa> Caixas { get; set; } = new List<Caixa>();
    public ICollection<PlanoContas> PlanoContas { get; set; } = new List<PlanoContas>();
    public ICollection<Categoria> Categorias { get; set; } = new List<Categoria>();
    public ICollection<Fornecedor> Fornecedores { get; set; } = new List<Fornecedor>();
    public ICollection<Cliente> Clientes { get; set; } = new List<Cliente>();
}

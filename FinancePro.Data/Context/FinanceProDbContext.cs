using FinancePro.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinancePro.Data.Context;

public class FinanceProDbContext : DbContext
{
    public FinanceProDbContext(DbContextOptions<FinanceProDbContext> options) : base(options)
    {
    }

    public DbSet<Empresa> Empresas => Set<Empresa>();
    public DbSet<ExercicioFinanceiro> ExerciciosFinanceiros => Set<ExercicioFinanceiro>();
    public DbSet<Moeda> Moedas => Set<Moeda>();
    public DbSet<Perfil> Perfis => Set<Perfil>();
    public DbSet<PermissaoPerfil> PermissoesPerfis => Set<PermissaoPerfil>();
    public DbSet<Utilizador> Utilizadores => Set<Utilizador>();
    public DbSet<Banco> Bancos => Set<Banco>();
    public DbSet<ContaBancaria> ContasBancarias => Set<ContaBancaria>();
    public DbSet<Caixa> Caixas => Set<Caixa>();
    public DbSet<SessaoCaixa> SessoesCaixa => Set<SessaoCaixa>();
    public DbSet<PlanoContas> PlanoContas => Set<PlanoContas>();
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Fornecedor> Fornecedores => Set<Fornecedor>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Movimento> Movimentos => Set<Movimento>();
    public DbSet<ContaReceber> ContasReceber => Set<ContaReceber>();
    public DbSet<ContaPagar> ContasPagar => Set<ContaPagar>();
    public DbSet<Orcamento> Orcamentos => Set<Orcamento>();
    public DbSet<OrcamentoDetalhe> OrcamentoDetalhes => Set<OrcamentoDetalhe>();
    public DbSet<RevisaoOrcamental> RevisoesOrcamentais => Set<RevisaoOrcamental>();
    public DbSet<Compra> Compras => Set<Compra>();
    public DbSet<LogAuditoria> LogsAuditoria => Set<LogAuditoria>();
    public DbSet<Bem> Bens => Set<Bem>();
    public DbSet<Produto> Produtos => Set<Produto>();
    public DbSet<MovimentoStock> MovimentosStock => Set<MovimentoStock>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FinanceProDbContext).Assembly);
    }
}

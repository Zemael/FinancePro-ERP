using FinancePro.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinancePro.Data.Configurations;

public class CompraConfiguration : IEntityTypeConfiguration<Compra>
{
    public void Configure(EntityTypeBuilder<Compra> builder)
    {
        builder.ToTable("Compras");
        builder.Property(c => c.NumeroPedido).IsRequired().HasMaxLength(20);
        builder.Property(c => c.Departamento).HasMaxLength(100);
        builder.Property(c => c.CentroCusto).HasMaxLength(100);
        builder.Property(c => c.Projeto).HasMaxLength(100);
        builder.Property(c => c.Comprador).HasMaxLength(150);
        builder.Property(c => c.Prioridade).HasConversion<string>().HasMaxLength(20);
        builder.Property(c => c.Estado).HasConversion<string>().HasMaxLength(20);
        builder.Property(c => c.ValorTotal).HasColumnType("decimal(18,2)");
        builder.Property(c => c.NumeroCotacao).HasMaxLength(30);
        builder.Property(c => c.NumeroOrdemCompra).HasMaxLength(30);

        builder.HasOne(c => c.Fornecedor).WithMany().HasForeignKey(c => c.FornecedorId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(c => c.Empresa).WithMany().HasForeignKey(c => c.EmpresaId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(c => new { c.EmpresaId, c.Data });
    }
}

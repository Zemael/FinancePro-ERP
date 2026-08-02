using FinancePro.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinancePro.Data.Configurations;

public class ContaPagarConfiguration : IEntityTypeConfiguration<ContaPagar>
{
    public void Configure(EntityTypeBuilder<ContaPagar> builder)
    {
        builder.ToTable("ContasPagar");
        builder.Property(c => c.Codigo).HasMaxLength(20);
        builder.Property(c => c.Descricao).IsRequired().HasMaxLength(200);
        builder.Property(c => c.Valor).HasColumnType("decimal(18,2)");
        builder.Property(c => c.Estado).HasConversion<string>().HasMaxLength(20);
        builder.Property(c => c.FormaPagamento).HasMaxLength(50);
        builder.Property(c => c.CentroCusto).HasMaxLength(100);

        builder.HasOne(c => c.Fornecedor).WithMany().HasForeignKey(c => c.FornecedorId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(c => c.Categoria).WithMany().HasForeignKey(c => c.CategoriaId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(c => c.Movimento).WithMany().HasForeignKey(c => c.MovimentoId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(c => c.Empresa).WithMany().HasForeignKey(c => c.EmpresaId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(c => new { c.EmpresaId, c.DataVencimento });
    }
}

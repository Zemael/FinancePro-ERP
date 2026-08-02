using FinancePro.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinancePro.Data.Configurations;

public class CategoriaConfiguration : IEntityTypeConfiguration<Categoria>
{
    public void Configure(EntityTypeBuilder<Categoria> builder)
    {
        builder.ToTable("Categorias");
        builder.Property(c => c.Nome).IsRequired().HasMaxLength(100);
        builder.Property(c => c.Tipo).HasConversion<string>().HasMaxLength(20);

        builder.HasOne(c => c.PlanoContas)
            .WithMany(p => p.Categorias)
            .HasForeignKey(c => c.PlanoContasId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(c => c.Empresa)
            .WithMany(e => e.Categorias)
            .HasForeignKey(c => c.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

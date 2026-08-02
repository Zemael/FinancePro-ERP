using FinancePro.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinancePro.Data.Configurations;

public class BemConfiguration : IEntityTypeConfiguration<Bem>
{
    public void Configure(EntityTypeBuilder<Bem> builder)
    {
        builder.ToTable("Bens");
        builder.Property(b => b.Codigo).IsRequired().HasMaxLength(20);
        builder.Property(b => b.NumeroPatrimonial).IsRequired().HasMaxLength(30);
        builder.Property(b => b.Descricao).IsRequired().HasMaxLength(200);
        builder.Property(b => b.Categoria).HasMaxLength(100);
        builder.Property(b => b.Marca).HasMaxLength(100);
        builder.Property(b => b.Modelo).HasMaxLength(100);
        builder.Property(b => b.Serie).HasMaxLength(100);
        builder.Property(b => b.Localizacao).HasMaxLength(150);
        builder.Property(b => b.Responsavel).HasMaxLength(150);
        builder.Property(b => b.ValorAquisicao).HasColumnType("decimal(18,2)");
        builder.Property(b => b.MetodoDepreciacao).HasConversion<string>().HasMaxLength(20);
        builder.Property(b => b.Estado).HasConversion<string>().HasMaxLength(20);

        builder.HasOne(b => b.Empresa).WithMany().HasForeignKey(b => b.EmpresaId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(b => new { b.EmpresaId, b.NumeroPatrimonial }).IsUnique();
    }
}

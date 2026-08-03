using FinancePro.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinancePro.Data.Configurations;

public class PlanoContasConfiguration : IEntityTypeConfiguration<PlanoContas>
{
    public void Configure(EntityTypeBuilder<PlanoContas> builder)
    {
        builder.ToTable("PlanoContas");
        builder.Property(p => p.Codigo).IsRequired().HasMaxLength(20);
        builder.Property(p => p.Nome).IsRequired().HasMaxLength(150);
        builder.Property(p => p.Tipo).HasConversion<string>().HasMaxLength(30);
        builder.Property(p => p.Natureza).HasConversion<string>().HasMaxLength(20);

        builder.HasOne(p => p.ContaPai)
            .WithMany(p => p.SubContas)
            .HasForeignKey(p => p.ContaPaiId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Empresa)
            .WithMany(e => e.PlanoContas)
            .HasForeignKey(p => p.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(p => new { p.EmpresaId, p.Codigo }).IsUnique();
    }
}

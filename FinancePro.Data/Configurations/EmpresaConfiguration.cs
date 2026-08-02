using FinancePro.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinancePro.Data.Configurations;

public class EmpresaConfiguration : IEntityTypeConfiguration<Empresa>
{
    public void Configure(EntityTypeBuilder<Empresa> builder)
    {
        builder.ToTable("Empresas");
        builder.Property(e => e.Nome).IsRequired().HasMaxLength(150);
        builder.Property(e => e.NIF).HasMaxLength(30);
        builder.Property(e => e.Moeda).IsRequired().HasMaxLength(20);
        builder.HasIndex(e => e.NIF).IsUnique(false);
    }
}

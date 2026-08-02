using FinancePro.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinancePro.Data.Configurations;

public class BancoConfiguration : IEntityTypeConfiguration<Banco>
{
    public void Configure(EntityTypeBuilder<Banco> builder)
    {
        builder.ToTable("Bancos");
        builder.Property(b => b.Nome).IsRequired().HasMaxLength(120);
        builder.Property(b => b.CodigoSwift).HasMaxLength(20);
    }
}

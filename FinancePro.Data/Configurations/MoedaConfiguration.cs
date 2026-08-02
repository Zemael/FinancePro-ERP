using FinancePro.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinancePro.Data.Configurations;

public sealed class MoedaConfiguration : IEntityTypeConfiguration<Moeda>
{
    public void Configure(EntityTypeBuilder<Moeda> builder)
    {
        builder.ToTable("Moedas");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.CodigoIso).IsUnique();
        builder.Property(x => x.CodigoIso).HasMaxLength(3).IsRequired();
        builder.Property(x => x.Nome).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Simbolo).HasMaxLength(10).IsRequired();
    }
}

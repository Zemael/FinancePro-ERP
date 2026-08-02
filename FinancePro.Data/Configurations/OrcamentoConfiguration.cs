using FinancePro.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinancePro.Data.Configurations;

public class OrcamentoConfiguration : IEntityTypeConfiguration<Orcamento>
{
    public void Configure(EntityTypeBuilder<Orcamento> builder)
    {
        builder.ToTable("Orcamentos");
        builder.Property(o => o.Nome).IsRequired().HasMaxLength(150);
        builder.Property(o => o.Moeda).IsRequired().HasMaxLength(20);
        builder.Property(o => o.Estado).HasConversion<string>().HasMaxLength(20);
        builder.Property(o => o.ElaboradoPor).HasMaxLength(150);
        builder.Property(o => o.RevistoPor).HasMaxLength(150);
        builder.Property(o => o.AprovadoPor).HasMaxLength(150);

        builder.HasOne(o => o.Empresa).WithMany().HasForeignKey(o => o.EmpresaId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(o => new { o.EmpresaId, o.Ano });
    }
}

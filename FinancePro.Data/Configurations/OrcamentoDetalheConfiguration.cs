using FinancePro.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinancePro.Data.Configurations;

public class OrcamentoDetalheConfiguration : IEntityTypeConfiguration<OrcamentoDetalhe>
{
    public void Configure(EntityTypeBuilder<OrcamentoDetalhe> builder)
    {
        builder.ToTable("OrcamentoDetalhes");
        builder.Property(d => d.CentroCusto).HasMaxLength(100);
        builder.Property(d => d.Departamento).HasMaxLength(100);
        builder.Property(d => d.Tipo).HasConversion<string>().HasMaxLength(20);
        builder.Property(d => d.ValorPrevisto).HasColumnType("decimal(18,2)");

        builder.HasOne(d => d.Orcamento).WithMany(o => o.Detalhes).HasForeignKey(d => d.OrcamentoId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(d => d.PlanoContas).WithMany().HasForeignKey(d => d.PlanoContasId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(d => new { d.OrcamentoId, d.Mes, d.Tipo });
    }
}

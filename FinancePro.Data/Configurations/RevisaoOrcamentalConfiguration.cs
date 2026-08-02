using FinancePro.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinancePro.Data.Configurations;

public class RevisaoOrcamentalConfiguration : IEntityTypeConfiguration<RevisaoOrcamental>
{
    public void Configure(EntityTypeBuilder<RevisaoOrcamental> builder)
    {
        builder.ToTable("RevisoesOrcamentais");
        builder.Property(r => r.Motivo).IsRequired().HasMaxLength(300);
        builder.Property(r => r.Responsavel).IsRequired().HasMaxLength(150);
        builder.Property(r => r.Estado).HasConversion<string>().HasMaxLength(20);

        builder.HasOne(r => r.Orcamento).WithMany(o => o.Revisoes).HasForeignKey(r => r.OrcamentoId).OnDelete(DeleteBehavior.Cascade);
    }
}

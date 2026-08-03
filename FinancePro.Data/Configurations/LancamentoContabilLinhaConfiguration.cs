using FinancePro.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinancePro.Data.Configurations;

public sealed class LancamentoContabilLinhaConfiguration : IEntityTypeConfiguration<LancamentoContabilLinha>
{
    public void Configure(EntityTypeBuilder<LancamentoContabilLinha> builder)
    {
        builder.ToTable("LancamentoContabilLinhas");
        builder.Property(x => x.Descricao).HasMaxLength(250);
        builder.Property(x => x.Debito).HasPrecision(18,2);
        builder.Property(x => x.Credito).HasPrecision(18,2);
        builder.Property(x => x.CentroCusto).HasMaxLength(80);
        builder.HasOne(x => x.LancamentoContabil).WithMany(x => x.Linhas).HasForeignKey(x => x.LancamentoContabilId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.PlanoContas).WithMany(x => x.LinhasLancamento).HasForeignKey(x => x.PlanoContasId).OnDelete(DeleteBehavior.Restrict);
    }
}

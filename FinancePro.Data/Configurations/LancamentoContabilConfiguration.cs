using FinancePro.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinancePro.Data.Configurations;

public sealed class LancamentoContabilConfiguration : IEntityTypeConfiguration<LancamentoContabil>
{
    public void Configure(EntityTypeBuilder<LancamentoContabil> builder)
    {
        builder.ToTable("LancamentosContabeis");
        builder.Property(x => x.Numero).IsRequired().HasMaxLength(30);
        builder.Property(x => x.Descricao).IsRequired().HasMaxLength(250);
        builder.Property(x => x.DocumentoReferencia).HasMaxLength(80);
        builder.Property(x => x.OrigemModulo).HasMaxLength(50);
        builder.Property(x => x.Estado).HasConversion<string>().HasMaxLength(30);
        builder.HasIndex(x => new { x.EmpresaId, x.Numero }).IsUnique();
        builder.HasOne(x => x.Empresa).WithMany().HasForeignKey(x => x.EmpresaId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Utilizador).WithMany().HasForeignKey(x => x.UtilizadorId).OnDelete(DeleteBehavior.Restrict);
    }
}

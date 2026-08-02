using FinancePro.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinancePro.Data.Configurations;

public class CaixaConfiguration : IEntityTypeConfiguration<Caixa>
{
    public void Configure(EntityTypeBuilder<Caixa> builder)
    {
        builder.ToTable("Caixas");
        builder.Property(c => c.Nome).IsRequired().HasMaxLength(100);
        builder.Property(c => c.SaldoInicial).HasColumnType("decimal(18,2)");
        builder.Property(c => c.SaldoMinimo).HasColumnType("decimal(18,2)");
        builder.Property(c => c.PermiteSaldoNegativo).HasDefaultValue(false);

        builder.HasOne(c => c.Empresa)
            .WithMany(e => e.Caixas)
            .HasForeignKey(c => c.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

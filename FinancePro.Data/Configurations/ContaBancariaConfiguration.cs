using FinancePro.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinancePro.Data.Configurations;

public class ContaBancariaConfiguration : IEntityTypeConfiguration<ContaBancaria>
{
    public void Configure(EntityTypeBuilder<ContaBancaria> builder)
    {
        builder.ToTable("ContasBancarias");
        builder.Property(c => c.NumeroConta).IsRequired().HasMaxLength(40);
        builder.Property(c => c.IBAN).HasMaxLength(40);
        builder.Property(c => c.Titular).IsRequired().HasMaxLength(150);
        builder.Property(c => c.SaldoInicial).HasColumnType("decimal(18,2)");
        builder.Property(c => c.Moeda).IsRequired().HasMaxLength(20);

        builder.HasOne(c => c.Banco)
            .WithMany(b => b.ContasBancarias)
            .HasForeignKey(c => c.BancoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.Empresa)
            .WithMany(e => e.ContasBancarias)
            .HasForeignKey(c => c.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

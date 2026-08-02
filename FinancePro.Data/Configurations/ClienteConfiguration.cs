using FinancePro.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinancePro.Data.Configurations;

public class ClienteConfiguration : IEntityTypeConfiguration<Cliente>
{
    public void Configure(EntityTypeBuilder<Cliente> builder)
    {
        builder.ToTable("Clientes");
        builder.Property(c => c.Nome).IsRequired().HasMaxLength(150);
        builder.Property(c => c.NIF).HasMaxLength(30);

        builder.HasOne(c => c.Empresa)
            .WithMany(e => e.Clientes)
            .HasForeignKey(c => c.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

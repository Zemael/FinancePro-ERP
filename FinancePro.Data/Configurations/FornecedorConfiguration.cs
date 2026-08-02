using FinancePro.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinancePro.Data.Configurations;

public class FornecedorConfiguration : IEntityTypeConfiguration<Fornecedor>
{
    public void Configure(EntityTypeBuilder<Fornecedor> builder)
    {
        builder.ToTable("Fornecedores");
        builder.Property(f => f.Nome).IsRequired().HasMaxLength(150);
        builder.Property(f => f.NIF).HasMaxLength(30);

        builder.HasOne(f => f.Empresa)
            .WithMany(e => e.Fornecedores)
            .HasForeignKey(f => f.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

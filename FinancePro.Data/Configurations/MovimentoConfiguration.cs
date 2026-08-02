using FinancePro.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinancePro.Data.Configurations;

public class MovimentoConfiguration : IEntityTypeConfiguration<Movimento>
{
    public void Configure(EntityTypeBuilder<Movimento> builder)
    {
        builder.ToTable("Movimentos");
        builder.Property(m => m.Descricao).IsRequired().HasMaxLength(200);
        builder.Property(m => m.Valor).HasColumnType("decimal(18,2)");
        builder.Property(m => m.Tipo).HasConversion<string>().HasMaxLength(20);
        builder.Property(m => m.TipoOperacao).HasConversion<string>().HasMaxLength(20);
        builder.Property(m => m.Estado).HasConversion<string>().HasMaxLength(20);
        builder.Property(m => m.FormaPagamento).HasMaxLength(50);
        builder.Property(m => m.CentroCusto).HasMaxLength(100);

        // Exatamente uma origem (CaixaId XOR ContaBancariaId) é garantido
        // pelo CHECK constraint no schema SQL e validado em TesourariaService
        // antes de gravar — não é possível expressar XOR em Fluent API.
        builder.HasOne(m => m.Categoria).WithMany().HasForeignKey(m => m.CategoriaId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(m => m.Caixa).WithMany().HasForeignKey(m => m.CaixaId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(m => m.ContaBancaria).WithMany().HasForeignKey(m => m.ContaBancariaId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(m => m.Empresa).WithMany().HasForeignKey(m => m.EmpresaId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(m => new { m.EmpresaId, m.Data });
    }
}

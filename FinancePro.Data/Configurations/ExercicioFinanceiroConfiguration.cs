using FinancePro.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinancePro.Data.Configurations;

public sealed class ExercicioFinanceiroConfiguration : IEntityTypeConfiguration<ExercicioFinanceiro>
{
    public void Configure(EntityTypeBuilder<ExercicioFinanceiro> builder)
    {
        builder.ToTable("ExerciciosFinanceiros");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => new { x.EmpresaId, x.Ano }).IsUnique();
        builder.Property(x => x.Ano).IsRequired();
        builder.Property(x => x.DataInicio).HasColumnType("date");
        builder.Property(x => x.DataFim).HasColumnType("date");
        builder.HasOne(x => x.Empresa)
            .WithMany(x => x.ExerciciosFinanceiros)
            .HasForeignKey(x => x.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

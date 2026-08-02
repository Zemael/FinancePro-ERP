using FinancePro.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinancePro.Data.Configurations;

public sealed class SessaoCaixaConfiguration : IEntityTypeConfiguration<SessaoCaixa>
{
    public void Configure(EntityTypeBuilder<SessaoCaixa> builder)
    {
        builder.ToTable("SessoesCaixa");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.SaldoInicial).HasColumnType("decimal(18,2)");
        builder.Property(x => x.SaldoContado).HasColumnType("decimal(18,2)");
        builder.Property(x => x.SaldoCalculado).HasColumnType("decimal(18,2)");
        builder.Property(x => x.Diferenca).HasColumnType("decimal(18,2)");
        builder.Property(x => x.ObservacaoAbertura).HasMaxLength(500);
        builder.Property(x => x.ObservacaoFecho).HasMaxLength(500);

        builder.HasOne(x => x.Caixa).WithMany().HasForeignKey(x => x.CaixaId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Empresa).WithMany().HasForeignKey(x => x.EmpresaId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.ExercicioFinanceiro).WithMany().HasForeignKey(x => x.ExercicioFinanceiroId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Utilizador).WithMany().HasForeignKey(x => x.UtilizadorId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.CaixaId, x.Estado });
    }
}

using FinancePro.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinancePro.Data.Configurations;

public class LogAuditoriaConfiguration : IEntityTypeConfiguration<LogAuditoria>
{
    public void Configure(EntityTypeBuilder<LogAuditoria> builder)
    {
        builder.ToTable("LogsAuditoria");
        builder.Property(l => l.Entidade).IsRequired().HasMaxLength(60);
        builder.Property(l => l.Acao).IsRequired().HasMaxLength(30);
        builder.Property(l => l.UtilizadorNome).IsRequired().HasMaxLength(150);

        builder.HasIndex(l => new { l.Entidade, l.RegistoId });
        builder.HasIndex(l => new { l.EmpresaId, l.Data });
    }
}

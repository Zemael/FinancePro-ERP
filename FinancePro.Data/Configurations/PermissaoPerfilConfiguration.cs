using FinancePro.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinancePro.Data.Configurations;

public sealed class PermissaoPerfilConfiguration : IEntityTypeConfiguration<PermissaoPerfil>
{
    public void Configure(EntityTypeBuilder<PermissaoPerfil> builder)
    {
        builder.ToTable("PermissoesPerfis");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Modulo).IsRequired().HasMaxLength(80);
        builder.HasIndex(x => new { x.PerfilId, x.Modulo }).IsUnique();
        builder.HasOne(x => x.Perfil)
            .WithMany(x => x.Permissoes)
            .HasForeignKey(x => x.PerfilId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

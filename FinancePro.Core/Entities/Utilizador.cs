namespace FinancePro.Core.Entities;

/// <summary>Utilizador do sistema — autenticado no ecrã de Login (Etapa 3).</summary>
public class Utilizador : EntityBase
{
    public string NomeCompleto { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public byte[]? FotoPerfil { get; set; }
    public DateTime? UltimoLogin { get; set; }

    public int PerfilId { get; set; }
    public Perfil Perfil { get; set; } = null!;

    public int EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = null!;
}

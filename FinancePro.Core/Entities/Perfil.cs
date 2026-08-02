namespace FinancePro.Core.Entities;

/// <summary>Perfil de acesso do utilizador (ex.: Administrador, Gestor, Operador).</summary>
public class Perfil : EntityBase
{
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }

    public ICollection<Utilizador> Utilizadores { get; set; } = new List<Utilizador>();
    public ICollection<PermissaoPerfil> Permissoes { get; set; } = new List<PermissaoPerfil>();
}

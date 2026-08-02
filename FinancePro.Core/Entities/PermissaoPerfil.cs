namespace FinancePro.Core.Entities;

/// <summary>Permissões concedidas a um perfil para um módulo do FinancePro.</summary>
public sealed class PermissaoPerfil : EntityBase
{
    public int PerfilId { get; set; }
    public Perfil Perfil { get; set; } = null!;

    public string Modulo { get; set; } = string.Empty;
    public bool Consultar { get; set; }
    public bool Criar { get; set; }
    public bool Editar { get; set; }
    public bool Desativar { get; set; }
    public bool Aprovar { get; set; }
    public bool Exportar { get; set; }
    public bool Administrar { get; set; }
}

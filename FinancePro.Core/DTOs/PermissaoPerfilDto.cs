namespace FinancePro.Core.DTOs;

public sealed class PermissaoPerfilDto
{
    public int Id { get; set; }
    public int PerfilId { get; set; }
    public string Modulo { get; set; } = string.Empty;
    public string DescricaoModulo { get; set; } = string.Empty;
    public bool Consultar { get; set; }
    public bool Criar { get; set; }
    public bool Editar { get; set; }
    public bool Desativar { get; set; }
    public bool Aprovar { get; set; }
    public bool Exportar { get; set; }
    public bool Administrar { get; set; }
    public override string ToString() => string.IsNullOrWhiteSpace(DescricaoModulo) ? Modulo : DescricaoModulo;
}

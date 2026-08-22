namespace FinancePro.Core.DTOs;

public sealed class PerfilDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public bool Ativo { get; set; } = true;

    /// <summary>
    /// Garante que controlos WPF exibem o nome do perfil quando não existe
    /// um DisplayMemberPath explícito, em vez do nome completo do tipo DTO.
    /// </summary>
    public override string ToString() => Nome;
}

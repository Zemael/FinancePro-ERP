namespace FinancePro.Core.DTOs;

/// <summary>Resultado de uma tentativa de autenticação.</summary>
public class LoginResultDto
{
    public bool Sucesso { get; set; }
    public string? Mensagem { get; set; }
    public int UtilizadorId { get; set; }
    public string NomeCompleto { get; set; } = string.Empty;
    public string PerfilNome { get; set; } = string.Empty;
    public int EmpresaId { get; set; }
    public IReadOnlyCollection<string> Permissoes { get; set; } = Array.Empty<string>();
}

namespace FinancePro.UI.Common;

/// <summary>Dados e permissões do utilizador autenticado na sessão atual.</summary>
public static class SessaoAtual
{
    private static readonly HashSet<string> _permissoes = new(StringComparer.OrdinalIgnoreCase);

    public static int UtilizadorId { get; private set; }
    public static string NomeCompleto { get; private set; } = string.Empty;
    public static string PerfilNome { get; private set; } = string.Empty;
    public static int EmpresaId { get; private set; }

    public static void Definir(int utilizadorId, string nomeCompleto, string perfilNome, int empresaId,
        IEnumerable<string>? permissoes = null)
    {
        UtilizadorId = utilizadorId;
        NomeCompleto = nomeCompleto;
        PerfilNome = perfilNome;
        EmpresaId = empresaId;
        _permissoes.Clear();
        if (permissoes is not null)
            foreach (var permissao in permissoes) _permissoes.Add(permissao);
    }

    public static bool TemPermissao(string modulo, string acao = "Consultar") =>
        PerfilNome.Equals("Administrador", StringComparison.OrdinalIgnoreCase) ||
        _permissoes.Contains("*") || _permissoes.Contains($"{modulo}.{acao}");

    public static bool PodeEliminarOuAprovar =>
        PerfilNome.Equals("Administrador", StringComparison.OrdinalIgnoreCase) ||
        _permissoes.Any(x => x.EndsWith(".Desativar", StringComparison.OrdinalIgnoreCase) ||
                             x.EndsWith(".Aprovar", StringComparison.OrdinalIgnoreCase));
}

using System.IO;
using System.Text.Json;

namespace FinancePro.UI.Common;

/// <summary>
/// Guarda localmente a preferência "Lembrar acesso" do ecrã de Login.
/// Por segurança, NUNCA guarda a palavra-passe — apenas o email, para
/// pré-preencher o campo na próxima abertura da aplicação.
/// </summary>
public static class PreferenciasLogin
{
    private static readonly string CaminhoFicheiro = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "FinancePro", "login.json");

    private class Dados
    {
        public string? Email { get; set; }
        public bool Lembrar { get; set; }
    }

    public static (string Email, bool Lembrar) Carregar()
    {
        try
        {
            if (!File.Exists(CaminhoFicheiro))
                return (string.Empty, false);

            var json = File.ReadAllText(CaminhoFicheiro);
            var dados = JsonSerializer.Deserialize<Dados>(json);
            return (dados?.Email ?? string.Empty, dados?.Lembrar ?? false);
        }
        catch
        {
            // Ficheiro corrompido ou inacessível — trata como se não houvesse preferência guardada.
            return (string.Empty, false);
        }
    }

    public static void Guardar(string email, bool lembrar)
    {
        try
        {
            var diretorio = Path.GetDirectoryName(CaminhoFicheiro)!;
            Directory.CreateDirectory(diretorio);

            if (!lembrar)
            {
                if (File.Exists(CaminhoFicheiro))
                    File.Delete(CaminhoFicheiro);
                return;
            }

            var dados = new Dados { Email = email, Lembrar = true };
            File.WriteAllText(CaminhoFicheiro, JsonSerializer.Serialize(dados));
        }
        catch
        {
            // Falha a persistir preferências não deve impedir o login.
        }
    }
}

using System.IO;
using System.Text;
using Microsoft.Win32;

namespace FinancePro.UI.Common;

/// <summary>Ação "Exportar" comum a todos os módulos: escreve as linhas
/// visíveis da grelha para um ficheiro CSV, com diálogo nativo de gravação.</summary>
public static class ExportadorCsv
{
    /// <param name="cabecalhos">Nomes das colunas.</param>
    /// <param name="linhas">Cada linha é uma lista de valores já formatados como texto.</param>
    /// <returns>True se gravou; false se o utilizador cancelou.</returns>
    public static bool Exportar(string nomeSugerido, IReadOnlyList<string> cabecalhos, IEnumerable<IReadOnlyList<string>> linhas)
    {
        var dialogo = new SaveFileDialog
        {
            FileName = nomeSugerido,
            DefaultExt = ".csv",
            Filter = "Ficheiro CSV (*.csv)|*.csv"
        };

        if (dialogo.ShowDialog() != true)
        {
            return false;
        }

        var sb = new StringBuilder();
        sb.AppendLine(string.Join(';', cabecalhos.Select(Escapar)));
        foreach (var linha in linhas)
        {
            sb.AppendLine(string.Join(';', linha.Select(Escapar)));
        }

        File.WriteAllText(dialogo.FileName, sb.ToString(), Encoding.UTF8);
        return true;
    }

    private static string Escapar(string valor)
    {
        valor ??= string.Empty;
        return valor.Contains(';') || valor.Contains('"') || valor.Contains('\n')
            ? $"\"{valor.Replace("\"", "\"\"")}\""
            : valor;
    }
}

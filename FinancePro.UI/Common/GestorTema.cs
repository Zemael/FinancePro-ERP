using System.Windows;

namespace FinancePro.UI.Common;

/// <summary>
/// Troca entre Theme.Light.xaml e Theme.Dark.xaml em tempo real. Como
/// Styles.xaml usa DynamicResource para todas as cores da paleta, trocar
/// o dicionário aqui atualiza a app inteira sem reiniciar nada.
/// </summary>
public static class GestorTema
{
    private const string CaminhoClaro = "Resources/Theme.Light.xaml";
    private const string CaminhoEscuro = "Resources/Theme.Dark.xaml";

    public static bool TemaEscuroAtivo { get; private set; }

    public static void Alternar()
    {
        TemaEscuroAtivo = !TemaEscuroAtivo;
        AplicarTema(TemaEscuroAtivo);
    }

    private static void AplicarTema(bool escuro)
    {
        var dicionarios = System.Windows.Application.Current.Resources.MergedDictionaries;

        var atual = dicionarios.FirstOrDefault(d =>
            d.Source is not null &&
            (d.Source.OriginalString.EndsWith("Theme.Light.xaml") || d.Source.OriginalString.EndsWith("Theme.Dark.xaml")));

        var novo = new ResourceDictionary
        {
            Source = new Uri(escuro ? CaminhoEscuro : CaminhoClaro, UriKind.Relative)
        };

        if (atual is not null)
        {
            var indice = dicionarios.IndexOf(atual);
            dicionarios.RemoveAt(indice);
            dicionarios.Insert(indice, novo);
        }
        else
        {
            dicionarios.Insert(0, novo);
        }
    }
}

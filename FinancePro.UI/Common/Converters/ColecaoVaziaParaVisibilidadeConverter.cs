using System.Collections;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace FinancePro.UI.Common.Converters;

public class ColecaoVaziaParaVisibilidadeConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        var vazia = value is not ICollection colecao || colecao.Count == 0;
        return vazia ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object? value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace FinancePro.UI.Common.Converters;

public class ObjetoParaVisibilidadeConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        var temValor = value is not null;
        if (string.Equals(parameter as string, "Inverso", StringComparison.OrdinalIgnoreCase))
        {
            temValor = !temValor;
        }
        return temValor ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object? value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace FinancePro.UI.Common.Converters;

public class ContagemParaVisibilidadeConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        var visivel = value is int i && i > 0;
        if (string.Equals(parameter as string, "Inverso", StringComparison.OrdinalIgnoreCase))
        {
            visivel = !visivel;
        }
        return visivel ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object? value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

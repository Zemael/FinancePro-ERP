using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace FinancePro.UI.Common.Converters;

public class BoolParaVisibilidadeConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        var visivel = value is true;
        if (string.Equals(parameter as string, "Inverso", StringComparison.OrdinalIgnoreCase))
        {
            visivel = !visivel;
        }
        return visivel ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object? value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

using System.Globalization;
using System.Windows.Data;

namespace FinancePro.UI.Common.Converters;

public class BusyToTextConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        var rotulo = parameter as string ?? "Entrar";
        return value is true ? $"A {rotulo.ToLowerInvariant()}..." : rotulo;
    }

    public object ConvertBack(object? value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

using System.Globalization;
using System.Windows.Data;

namespace FinancePro.UI.Common.Converters;

public class ValorMoedaConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        var valor = value is decimal d ? d : 0m;
        return $"{valor:#,##0} FCFA";
    }

    public object ConvertBack(object? value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

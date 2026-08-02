using System.Globalization;
using System.Windows.Data;

namespace FinancePro.UI.Common.Converters;

public class AtivoParaTextoConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object parameter, CultureInfo culture) =>
        value is true ? "Ativa" : "Inativa";

    public object ConvertBack(object? value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

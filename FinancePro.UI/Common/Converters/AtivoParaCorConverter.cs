using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace FinancePro.UI.Common.Converters;

public class AtivoParaCorConverter : IValueConverter
{
    private static readonly Brush CorAtivo = new SolidColorBrush(Color.FromRgb(0x0F, 0x6E, 0x56));
    private static readonly Brush CorInativo = new SolidColorBrush(Color.FromRgb(0xC0, 0x00, 0x00));

    public object Convert(object? value, Type targetType, object parameter, CultureInfo culture) =>
        value is true ? CorAtivo : CorInativo;

    public object ConvertBack(object? value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

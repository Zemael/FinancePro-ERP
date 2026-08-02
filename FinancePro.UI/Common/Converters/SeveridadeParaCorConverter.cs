using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace FinancePro.UI.Common.Converters;

public class SeveridadeParaCorConverter : IValueConverter
{
    private static readonly Brush Critico = new SolidColorBrush(Color.FromRgb(0xC0, 0x00, 0x00));
    private static readonly Brush Aviso = new SolidColorBrush(Color.FromRgb(0xE9, 0x71, 0x32));
    private static readonly Brush Info = new SolidColorBrush(Color.FromRgb(0x59, 0x59, 0x59));

    public object Convert(object? value, Type targetType, object parameter, CultureInfo culture) =>
        (value as string) switch
        {
            "Critico" => Critico,
            "Aviso" => Aviso,
            _ => Info
        };

    public object ConvertBack(object? value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

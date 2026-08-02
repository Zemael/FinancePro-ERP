using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace FinancePro.UI.Common.Converters;

public class EstadoContaParaCorConverter : IValueConverter
{
    private static readonly Brush Recebido = new SolidColorBrush(Color.FromRgb(0x0F, 0x6E, 0x56));
    private static readonly Brush Atrasado = new SolidColorBrush(Color.FromRgb(0xC0, 0x00, 0x00));
    private static readonly Brush Pendente = new SolidColorBrush(Color.FromRgb(0xE9, 0x71, 0x32));
    private static readonly Brush Cancelado = new SolidColorBrush(Color.FromRgb(0x88, 0x87, 0x80));

    public object Convert(object? value, Type targetType, object parameter, CultureInfo culture) =>
        (value as string) switch
        {
            "Recebido" => Recebido,
            "Paga" => Recebido,
            "Atrasado" => Atrasado,
            "Cancelado" => Cancelado,
            "Cancelada" => Cancelado,
            _ => Pendente
        };

    public object ConvertBack(object? value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

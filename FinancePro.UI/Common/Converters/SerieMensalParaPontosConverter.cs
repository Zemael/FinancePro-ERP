using System.Collections;
using System.Globalization;
using System.Windows.Data;
using FinancePro.Core.DTOs;

namespace FinancePro.UI.Common.Converters;

/// <summary>Converte a coleção de <see cref="SerieMensalDto"/> do Dashboard numa string de
/// pontos "x,y x,y ..." para um <see cref="System.Windows.Shapes.Polyline"/>, normalizando
/// os valores ao espaço de desenho 520x160 usado no gráfico de fluxo de caixa.
/// ConverterParameter indica o campo: "Saldo" | "Entradas" | "Saidas".</summary>
public sealed class SerieMensalParaPontosConverter : IValueConverter
{
    private const double Largura = 520;
    private const double AlturaTopo = 20;
    private const double AlturaBase = 150;

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not IEnumerable serie) return string.Empty;
        var pontos = serie.Cast<SerieMensalDto>().ToList();
        if (pontos.Count == 0) return string.Empty;

        var campo = parameter as string ?? "Saldo";
        var valores = pontos.Select(p => campo switch
        {
            "Entradas" => p.Entradas,
            "Saidas" => p.Saidas,
            _ => p.SaldoAcumulado
        }).ToList();

        var min = valores.Min();
        var max = valores.Max();
        var amplitude = max - min;
        if (amplitude == 0) amplitude = 1;

        var passoX = pontos.Count > 1 ? Largura / (pontos.Count - 1) : 0;
        var partes = new List<string>();
        for (var i = 0; i < pontos.Count; i++)
        {
            var x = i * passoX;
            var proporcao = (double)((valores[i] - min) / amplitude);
            var y = AlturaBase - (proporcao * (AlturaBase - AlturaTopo));
            partes.Add($"{x.ToString(CultureInfo.InvariantCulture)},{y.ToString(CultureInfo.InvariantCulture)}");
        }
        return string.Join(" ", partes);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

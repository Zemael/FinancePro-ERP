using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace FinancePro.UI.Common;

/// <summary>
/// Border que recorta o conteúdo usando os cantos superiores da sua dimensão atual.
/// O recorte é recalculado em cada alteração de layout, incluindo a expansão do menu lateral.
/// </summary>
public sealed class RoundedClipBorder : Border
{
    protected override Size ArrangeOverride(Size finalSize)
    {
        var arranged = base.ArrangeOverride(finalSize);
        UpdateClip(arranged);
        return arranged;
    }

    private void UpdateClip(Size size)
    {
        if (size.Width <= 0 || size.Height <= 0)
        {
            Clip = Geometry.Empty;
            return;
        }

        var leftRadius = Math.Min(Math.Max(0, CornerRadius.TopLeft), Math.Min(size.Width / 2, size.Height));
        var rightRadius = Math.Min(Math.Max(0, CornerRadius.TopRight), Math.Min(size.Width / 2, size.Height));

        var geometry = new StreamGeometry();
        using (var context = geometry.Open())
        {
            context.BeginFigure(new Point(0, leftRadius), true, true);

            if (leftRadius > 0)
                context.ArcTo(new Point(leftRadius, 0), new Size(leftRadius, leftRadius), 0, false,
                    SweepDirection.Clockwise, true, false);
            else
                context.LineTo(new Point(0, 0), true, false);

            context.LineTo(new Point(size.Width - rightRadius, 0), true, false);

            if (rightRadius > 0)
                context.ArcTo(new Point(size.Width, rightRadius), new Size(rightRadius, rightRadius), 0, false,
                    SweepDirection.Clockwise, true, false);
            else
                context.LineTo(new Point(size.Width, 0), true, false);

            context.LineTo(new Point(size.Width, size.Height), true, false);
            context.LineTo(new Point(0, size.Height), true, false);
        }

        geometry.Freeze();
        Clip = geometry;
    }
}

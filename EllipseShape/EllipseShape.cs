using Contract;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Shapes;

public class EllipseShape : PShape, IShape
{
    public string Name => "Ellipse";
    public string Icon => "Assets/Shapes/ellipse.png";

    public void HandleStart(double x, double y)
    {
        LeftTop.X = x;
        LeftTop.Y = y;
    }

    public void HandleEnd(double x, double y)
    {
        RightBottom.X = x;
        RightBottom.Y = y;
    }

    public UIElement Draw(SolidColorBrush brush, int thickness, DoubleCollection dash)
    {
        var left = Math.Min(RightBottom.X, LeftTop.X);
        var top = Math.Min(RightBottom.Y, LeftTop.Y);

        var right = Math.Max(RightBottom.X, LeftTop.X);
        var bottom = Math.Max(RightBottom.Y, LeftTop.Y);

        var width = right - left;
        var height = bottom - top;

        var ellipse = new Ellipse()
        {
            Width = width,
            Height = height,
            Stroke = brush,

            StrokeThickness = thickness,
            StrokeDashArray = dash

        };

        Canvas.SetLeft(ellipse, left);
        Canvas.SetTop(ellipse, top);

        RotateTransform transform = new(RotateAngle)
        {
            CenterX = width * 1.0 / 2,
            CenterY = height * 1.0 / 2
        };

        ellipse.RenderTransform = transform;
        return ellipse;
    }

    public IShape Clone()
    {
        return new EllipseShape();
    }

    override public PShape DeepCopy()
    {
        EllipseShape temp = new()
        {
            LeftTop = this.LeftTop.DeepCopy(),
            RightBottom = this.RightBottom.DeepCopy(),
            RotateAngle = RotateAngle,
            Thickness = this.Thickness
        };

        if (this.Brush != null)
            temp.Brush = this.Brush.Clone();

        if (this.StrokeDash != null)
            temp.StrokeDash = this.StrokeDash.Clone();

        return temp;
    }
}

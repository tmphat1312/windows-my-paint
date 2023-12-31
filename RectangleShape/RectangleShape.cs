using Contract;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Shapes;

public class RectangleShape : PShape, IShape
{
    public string Icon => "Assets/Shapes/rectangle.png";
    public string Name => "Rectangle";

    public void HandleStart(double x, double y)
    {
        LeftTop = new Point2D() { X = x, Y = y };
    }

    public void HandleEnd(double x, double y)
    {
        RightBottom = new Point2D() { X = x, Y = y };
    }

    public UIElement Draw(SolidColorBrush brush, int thickness, DoubleCollection dash)
    {
        var left = Math.Min(RightBottom.X, LeftTop.X);
        var top = Math.Min(RightBottom.Y, LeftTop.Y);
        var right = Math.Max(RightBottom.X, LeftTop.X);
        var bottom = Math.Max(RightBottom.Y, LeftTop.Y);
        var width = right - left;
        var height = bottom - top;

        var rect = new Rectangle()
        {
            Width = width,
            Height = height,
            StrokeThickness = thickness,
            Stroke = brush,
            StrokeDashArray = dash
        };

        Canvas.SetLeft(rect, left);
        Canvas.SetTop(rect, top);

        RotateTransform transform = new(this.RotateAngle)
        {
            CenterX = width * 1.0 / 2,
            CenterY = height * 1.0 / 2
        };

        rect.RenderTransform = transform;

        return rect;
    }

    public IShape Clone()
    {
        return new RectangleShape();
    }

    override public PShape DeepCopy()
    {
        RectangleShape temp = new RectangleShape();

        temp.LeftTop = this.LeftTop.DeepCopy();
        temp.RightBottom = this.RightBottom.DeepCopy();
        temp.RotateAngle = this.RotateAngle;
        temp.Thickness = this.Thickness;

        if (this.Brush != null)
            temp.Brush = this.Brush.Clone();

        if (this.StrokeDash != null)
            temp.StrokeDash = this.StrokeDash.Clone();

        return temp;
    }
}

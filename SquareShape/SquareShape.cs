using Contract;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Shapes;

public class SquareShape : PShape, IShape
{
    public string Name => "Square";

    public string Icon => "Assets/Shapes/square.png";

    public void HandleStart(double x, double y)
    {
        LeftTop.X = x;
        LeftTop.Y = y;
    }
    public void HandleEnd(double x, double y)
    {
        RightBottom.X = x;
        RightBottom.Y = y;

        double width = Math.Abs(RightBottom.X - LeftTop.X);
        double height = Math.Abs(RightBottom.Y - LeftTop.Y);

        if (width < height)
        {
            if (RightBottom.Y < LeftTop.Y)
                RightBottom.Y = LeftTop.Y - width;
            else
                RightBottom.Y = LeftTop.Y + width;
        }
        else
        if (width > height)
        {
            if (RightBottom.X < LeftTop.X)
                RightBottom.X = LeftTop.X - height;
            else RightBottom.X = LeftTop.X + height;
        }
    }

    public UIElement Draw(SolidColorBrush brush, int thickness, DoubleCollection dash)
    {
        double width = Math.Abs(RightBottom.X - LeftTop.X);
        double height = Math.Abs(RightBottom.Y - LeftTop.Y);

        var square = new Rectangle()
        {
            Width = width,
            Height = height,
            Stroke = brush,
            StrokeThickness = thickness,
            StrokeDashArray = dash
        };

        if (RightBottom.X > LeftTop.X && RightBottom.Y > LeftTop.Y)
        {
            Canvas.SetLeft(square, LeftTop.X);
            Canvas.SetTop(square, LeftTop.Y);
        }
        else if (RightBottom.X < LeftTop.X && RightBottom.Y > LeftTop.Y)
        {
            Canvas.SetLeft(square, RightBottom.X);
            Canvas.SetTop(square, LeftTop.Y);
        }
        else if (RightBottom.X > LeftTop.X && RightBottom.Y < LeftTop.Y)
        {
            Canvas.SetLeft(square, LeftTop.X);
            Canvas.SetTop(square, RightBottom.Y);
        }
        else
        {
            Canvas.SetLeft(square, RightBottom.X);
            Canvas.SetTop(square, RightBottom.Y);
        }

        RotateTransform transform = new(this.RotateAngle)
        {
            CenterX = width * 1.0 / 2,
            CenterY = height * 1.0 / 2
        };

        square.RenderTransform = transform;

        return square;
    }

    public IShape Clone()
    {
        return new SquareShape();
    }

    override public PShape DeepCopy()
    {
        SquareShape temp = new()
        {
            LeftTop = this.LeftTop.DeepCopy(),
            RightBottom = this.RightBottom.DeepCopy(),
            RotateAngle = this.RotateAngle,
            Thickness = this.Thickness
        };

        if (this.Brush != null)
            temp.Brush = this.Brush.Clone();

        if (this.StrokeDash != null)
            temp.StrokeDash = this.StrokeDash.Clone();

        return temp;
    }
}

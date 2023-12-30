using Contract;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Shapes;

public class Circle2D : CShape, IShape
{

    public DoubleCollection StrokeDash { get; set; }


    public SolidColorBrush Brush { get; set; }
    public string Name => "Circle";
    public string Icon => "Assets/Shapes/circle.png";
    public int Thickness { get; set; }

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

        var circle = new Ellipse()
        {
            Width = width,
            Height = height,
            StrokeThickness = thickness,
            Stroke = brush,
            StrokeDashArray = dash,
        };

        if (RightBottom.X > LeftTop.X && RightBottom.Y > LeftTop.Y)
        {
            Canvas.SetLeft(circle, LeftTop.X);
            Canvas.SetTop(circle, LeftTop.Y);
        }
        else if (RightBottom.X < LeftTop.X && RightBottom.Y > LeftTop.Y)
        {
            Canvas.SetLeft(circle, RightBottom.X);
            Canvas.SetTop(circle, LeftTop.Y);
        }
        else if (RightBottom.X > LeftTop.X && RightBottom.Y < LeftTop.Y)
        {
            Canvas.SetLeft(circle, LeftTop.X);
            Canvas.SetTop(circle, RightBottom.Y);
        }
        else
        {
            Canvas.SetLeft(circle, RightBottom.X);
            Canvas.SetTop(circle, RightBottom.Y);
        }

        return circle;
    }

    public IShape Clone()
    {
        return new Circle2D();
    }
    override public CShape DeepCopy()
    {
        Circle2D temp = new Circle2D();

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

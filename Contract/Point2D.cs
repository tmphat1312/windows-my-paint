using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Contract;

public class Point2D : IShape
{
    public double X { get; set; }
    public double Y { get; set; }

    public string Icon { get; } = "M 0 0 L 0 0";
    public string Name => "Point";

    public SolidColorBrush Brush { get; set; } = Brushes.Black;
    public DoubleCollection StrokeDash { get; set; } = new DoubleCollection();
    public int Thickness { get; set; }

    //public bool isHovering(double x, double y)
    //{
    //    return false;
    //}

    public Point2D()
    {
        X = 0;
        Y = 0;
    }

    public Point2D(double x, double y)
    {
        X = x;
        Y = y;
    }

    public void HandleStart(double x, double y)
    {
        X = x;
        Y = y;
    }

    public void HandleEnd(double x, double y)
    {
        X = x;
        Y = y;
    }

    public UIElement Draw(SolidColorBrush brush, int thickness, DoubleCollection dash)
    {
        Line line = new()
        {
            X1 = X,
            Y1 = Y,
            X2 = X,
            Y2 = Y,
            StrokeThickness = thickness,
            Stroke = brush,
            StrokeDashArray = dash
        };

        return line;
    }


    public IShape Clone()
    {
        return new Point2D();
    }

    public Point2D DeepCopy()
    {
        Point2D cloned = new()
        {
            Y = this.Y,
            X = this.X
        };

        return cloned;
    }
}

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Contract;

public class ControlPoint
{
    protected const int SIZE = 8;

    public Point2D Point { get; set; } = new();
    public Point2D CentrePoint { get; set; } = new();

    virtual public string Type { get; set; } = "rotate";

    //virtual public string Edge { get; set; } = "topleft";

    public ControlPoint()
    {
    }

    virtual public UIElement DrawPoint(double angle, Point2D centrePoint)
    {
        UIElement element = new Ellipse()
        {
            Width = SIZE,
            Height = SIZE,
            Fill = Brushes.White,
            Stroke = Brushes.Black,
            StrokeThickness = SIZE / 5,
        };

        Point pos = new() { X = Point.X, Y = Point.Y };
        Point centre = new() { X = CentrePoint.X, Y = CentrePoint.Y };

        Point afterTransform = VectorTranform.Rotate(pos, angle, centre);

        Canvas.SetLeft(element, afterTransform.X - SIZE / 2);
        Canvas.SetTop(element, afterTransform.Y - SIZE / 2);

        return element;
    }
    virtual public bool IsHovering(double angle, double x, double y)
    {
        Point pos = new() { X = Point.X, Y = Point.Y };
        Point centre = new() { X = CentrePoint.X, Y = CentrePoint.Y };

        Point afterTransform = VectorTranform.Rotate(pos, angle, centre);

        return util.isBetween(x, afterTransform.X + 15, afterTransform.X - 15)
            && util.isBetween(y, afterTransform.Y + 15, afterTransform.Y - 15);
    }

    virtual public string getEdge(double angle)
    {
        string[] edge = { "topleft", "topright", "bottomright", "bottomleft" };
        int index;
        if (Point.X > CentrePoint.X)
            if (Point.Y > CentrePoint.Y)
                index = 2;
            else
                index = 1;
        else
            if (Point.Y > CentrePoint.Y)
            index = 3;
        else
            index = 0;

        double rot = angle;

        if (rot > 0)
            while (true)
            {
                rot -= 90;
                if (rot < 0)
                    break;
                index++;

                if (index == 4)
                    index = 0;
            }
        else
            while (true)
            {
                rot += 90;
                if (rot > 0)
                    break;
                index--;
                if (index == -1)
                    index = 3;
            };

        return edge[index];
    }

    virtual public Point2D Handle(double angle, double x, double y)
    {
        Point2D result = new()
        {
            //result.X = Math.Cos(angle) * x + Math.Sin(angle) * y;
            //result.Y = Math.Cos(angle) * y + Math.Sin(angle) * x;
            X = x,
            Y = y
        };

        return result;
    }
}

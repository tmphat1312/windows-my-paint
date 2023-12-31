using Contract;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Shapes;

public class LineShape : PShape, IShape
{
    public string Name => "Line";
    public string Icon => "Assets/Shapes/line.png";

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
        Line line = new()
        {
            X1 = LeftTop.X,
            Y1 = LeftTop.Y,
            X2 = RightBottom.X,
            Y2 = RightBottom.Y,
            StrokeThickness = thickness,
            Stroke = brush,
            StrokeDashArray = dash
        };

        RotateTransform transform = new(this.RotateAngle);

        line.RenderTransform = transform;

        return line;
    }
    override public List<ControlPoint> GetControlPoints()
    {
        List<ControlPoint> controlPoints = [];

        ControlPoint leftTopEndPoint = new EndPoint()
        {
            Point = LeftTop,
            CentrePoint = this.GetCenterPoint(),
            Edge = "leftTop"
        };

        ControlPoint rightBottomEndPoint = new EndPoint()
        {
            Point = RightBottom,
            CentrePoint = this.GetCenterPoint(),
            Edge = "rightBottom"
        };

        ControlPoint moveControlPoint = new ControlPoint();
        moveControlPoint.Point = new Point2D() { X = (LeftTop.X + RightBottom.X) / 2, Y = (LeftTop.Y + RightBottom.Y) / 2 };
        moveControlPoint.Type = "move";
        moveControlPoint.CentrePoint = this.GetCenterPoint();

        controlPoints.Add(leftTopEndPoint);
        controlPoints.Add(rightBottomEndPoint);
        controlPoints.Add(moveControlPoint);


        return controlPoints;
    }

    override public UIElement ControlOutline()
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
            StrokeThickness = 2,
            Stroke = Brushes.Black,
            StrokeDashArray = { 4, 2, 4 }
        };

        var line = new Line()
        {
            X1 = LeftTop.X,
            Y1 = LeftTop.Y,
            X2 = RightBottom.X,
            Y2 = RightBottom.Y,
            StrokeThickness = 2,
            Stroke = Brushes.Black,
            StrokeDashArray = { 4, 2, 4 }
        };

        //Canvas.SetLeft(line, left);
        //Canvas.SetTop(line, top);

        RotateTransform transform = new RotateTransform(RotateAngle);
        transform.CenterX = width * 1.0 / 2;
        transform.CenterY = height * 1.0 / 2;

        line.RenderTransform = transform;

        return line;
    }

    public IShape Clone()
    {
        return new LineShape();
    }

    override public PShape DeepCopy()
    {
        LineShape temp = new()
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

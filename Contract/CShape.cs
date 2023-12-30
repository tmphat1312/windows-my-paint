using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Contract;

public class CShape
{
    public Point2D LeftTop { get; set; } = new();
    public Point2D RightBottom { get; set; } = new();
    public double RotateAngle { get; set; } = 0;

    virtual public Point2D GetCenterPoint()
    {
        Point2D centerPoint = new()
        {
            X = ((LeftTop.X + RightBottom.X) / 2),
            Y = ((LeftTop.Y + RightBottom.Y) / 2)
        };

        return centerPoint;
    }

    virtual public bool IsHovering(double x, double y)
    {
        return Util.IsBetween(x, this.RightBottom.X, this.LeftTop.X)
            && Util.IsBetween(y, this.RightBottom.Y, this.LeftTop.Y);
    }

    virtual public List<ControlPoint> GetControlPoints()
    {
        List<ControlPoint> controlPoints = [];

        ControlPoint diagPointTopLeft = new DiagPoint();
        //diagPointTopLeft.setPoint(LeftTop.X, LeftTop.Y);
        diagPointTopLeft.Point = LeftTop;

        ControlPoint diagPointBottomLeft = new DiagPoint();
        //diagPointBottomLeft.setPoint(LeftTop.X, RightBottom.Y);
        diagPointBottomLeft.Point = new Point2D() { X = LeftTop.X, Y = RightBottom.Y };

        ControlPoint diagPointTopRight = new DiagPoint();
        //diagPointTopRight.setPoint(RightBottom.X, LeftTop.Y);
        diagPointTopRight.Point = new Point2D() { X = RightBottom.X, Y = LeftTop.Y };

        ControlPoint diagPointBottomRight = new DiagPoint();
        //diagPointBottomRight.setPoint(RightBottom.X, RightBottom.Y);
        diagPointBottomRight.Point = RightBottom;

        //one way control Point

        ControlPoint diagPointRight = new OneSidePoint();
        //diagPointRight.setPoint(RightBottom.X, (RightBottom.Y + LeftTop.Y) / 2);
        diagPointRight.Point = new Point2D() { X = RightBottom.X, Y = (RightBottom.Y + LeftTop.Y) / 2 };

        ControlPoint diagPointLeft = new OneSidePoint();
        //diagPointLeft.setPoint(LeftTop.X, (RightBottom.Y + LeftTop.Y) / 2);
        diagPointLeft.Point = new Point2D() { X = LeftTop.X, Y = (RightBottom.Y + LeftTop.Y) / 2 };

        ControlPoint diagPointTop = new OneSidePoint();
        //diagPointTop.setPoint((LeftTop.X + RightBottom.X) / 2, LeftTop.Y);
        diagPointTop.Point = new Point2D() { X = (LeftTop.X + RightBottom.X) / 2, Y = LeftTop.Y };

        ControlPoint diagPointBottom = new OneSidePoint();
        //diagPointBottom.setPoint((LeftTop.X + RightBottom.X) / 2, RightBottom.Y);
        diagPointBottom.Point = new Point2D() { X = (LeftTop.X + RightBottom.X) / 2, Y = RightBottom.Y };


        ControlPoint angleControlPoint = new RotatePoint();
        //angleControlPoint.setPoint((RightBottom.X + LeftTop.X) / 2, Math.Min(RightBottom.Y, LeftTop.Y) - 50);
        angleControlPoint.Point = new Point2D() { X = (RightBottom.X + LeftTop.X) / 2, Y = Math.Min(RightBottom.Y, LeftTop.Y) - 50 };

        ControlPoint moveControlPoint = new ControlPoint();
        //moveControlPoint.setPoint((LeftTop.X + RightBottom.X) / 2, (LeftTop.Y + RightBottom.Y) / 2);
        moveControlPoint.Point = new Point2D() { X = (LeftTop.X + RightBottom.X) / 2, Y = (LeftTop.Y + RightBottom.Y) / 2 };
        moveControlPoint.Type = "move";

        controlPoints.Add(diagPointTopLeft);
        controlPoints.Add(diagPointTopRight);
        controlPoints.Add(diagPointBottomLeft);
        controlPoints.Add(diagPointBottomRight);

        controlPoints.Add(diagPointRight);
        controlPoints.Add(diagPointLeft);
        controlPoints.Add(diagPointBottom);
        controlPoints.Add(diagPointTop);

        controlPoints.Add(angleControlPoint);
        controlPoints.Add(moveControlPoint);

        return controlPoints;
    }

    virtual public UIElement ControlOutline()
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

        Canvas.SetLeft(rect, left);
        Canvas.SetTop(rect, top);

        RotateTransform transform = new RotateTransform(RotateAngle);
        transform.CenterX = width * 1.0 / 2;
        transform.CenterY = height * 1.0 / 2;

        rect.RenderTransform = transform;

        return rect;
    }

    virtual public CShape DeepCopy()
    {
        return new CShape()
        {
            LeftTop = this.LeftTop,
            RightBottom = this.RightBottom,
            RotateAngle = this.RotateAngle,
        };
    }
}

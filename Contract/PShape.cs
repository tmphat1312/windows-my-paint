﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Contract;

public class PShape
{
    public DoubleCollection? StrokeDash { get; set; } = null;
    public SolidColorBrush Brush { get; set; } = Brushes.Black;
    public int Thickness { get; set; } = 1;

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
        diagPointTopLeft.CentrePoint = this.GetCenterPoint();

        ControlPoint diagPointBottomLeft = new DiagPoint();
        //diagPointBottomLeft.setPoint(LeftTop.X, RightBottom.Y);
        diagPointBottomLeft.Point = new Point2D() { X = LeftTop.X, Y = RightBottom.Y };
        diagPointBottomLeft.CentrePoint = this.GetCenterPoint();

        ControlPoint diagPointTopRight = new DiagPoint();
        //diagPointTopRight.setPoint(RightBottom.X, LeftTop.Y);
        diagPointTopRight.Point = new Point2D() { X = RightBottom.X, Y = LeftTop.Y };
        diagPointTopRight.CentrePoint = this.GetCenterPoint();

        ControlPoint diagPointBottomRight = new DiagPoint();
        //diagPointBottomRight.setPoint(RightBottom.X, RightBottom.Y);
        diagPointBottomRight.Point = RightBottom;
        diagPointBottomRight.CentrePoint = this.GetCenterPoint();

        //one way control Point

        ControlPoint diagPointRight = new OneSidePoint();
        //diagPointRight.setPoint(RightBottom.X, (RightBottom.Y + LeftTop.Y) / 2);
        diagPointRight.Point = new Point2D() { X = RightBottom.X, Y = (RightBottom.Y + LeftTop.Y) / 2 };
        diagPointRight.CentrePoint = this.GetCenterPoint();

        ControlPoint diagPointLeft = new OneSidePoint();
        //diagPointLeft.setPoint(LeftTop.X, (RightBottom.Y + LeftTop.Y) / 2);
        diagPointLeft.Point = new Point2D() { X = LeftTop.X, Y = (RightBottom.Y + LeftTop.Y) / 2 };
        diagPointLeft.CentrePoint = this.GetCenterPoint();

        ControlPoint diagPointTop = new OneSidePoint();
        //diagPointTop.setPoint((LeftTop.X + RightBottom.X) / 2, LeftTop.Y);
        diagPointTop.Point = new Point2D() { X = (LeftTop.X + RightBottom.X) / 2, Y = LeftTop.Y };
        diagPointTop.CentrePoint = this.GetCenterPoint();

        ControlPoint diagPointBottom = new OneSidePoint();
        //diagPointBottom.setPoint((LeftTop.X + RightBottom.X) / 2, RightBottom.Y);
        diagPointBottom.Point = new Point2D() { X = (LeftTop.X + RightBottom.X) / 2, Y = RightBottom.Y };
        diagPointBottom.CentrePoint = this.GetCenterPoint();


        ControlPoint angleControlPoint = new RotatePoint();
        //angleControlPoint.setPoint((RightBottom.X + LeftTop.X) / 2, Math.Min(RightBottom.Y, LeftTop.Y) - 50);
        angleControlPoint.Point = new Point2D() { X = (RightBottom.X + LeftTop.X) / 2, Y = Math.Min(RightBottom.Y, LeftTop.Y) - 50 };
        angleControlPoint.CentrePoint = this.GetCenterPoint();

        ControlPoint moveControlPoint = new ControlPoint();
        //moveControlPoint.setPoint((LeftTop.X + RightBottom.X) / 2, (LeftTop.Y + RightBottom.Y) / 2);
        moveControlPoint.Point = new Point2D() { X = (LeftTop.X + RightBottom.X) / 2, Y = (LeftTop.Y + RightBottom.Y) / 2 };
        moveControlPoint.Type = "move";
        moveControlPoint.CentrePoint = this.GetCenterPoint();

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

    virtual public PShape DeepCopy()
    {
        return new PShape()
        {
            LeftTop = this.LeftTop,
            RightBottom = this.RightBottom,
            RotateAngle = this.RotateAngle,
        };
    }
}

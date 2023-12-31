using Contract;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Shapes
{
    public class Line2D : PShape, IShape
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

            ControlPoint diagPointTopLeft = new DiagPoint
            {
                Point = LeftTop
            };

            ControlPoint diagPointBottomLeft = new DiagPoint
            {
                Point = new Point2D() { X = LeftTop.X, Y = RightBottom.Y }
            };

            ControlPoint diagPointTopRight = new DiagPoint
            {
                Point = new Point2D() { X = RightBottom.X, Y = LeftTop.Y }
            };

            ControlPoint diagPointBottomRight = new DiagPoint
            {
                Point = RightBottom
            };

            ControlPoint diagPointRight = new OneSidePoint
            {
                Point = new Point2D() { X = RightBottom.X, Y = (RightBottom.Y + LeftTop.Y) / 2 }
            };

            ControlPoint diagPointLeft = new OneSidePoint
            {
                Point = new Point2D() { X = LeftTop.X, Y = (RightBottom.Y + LeftTop.Y) / 2 }
            };


            ControlPoint diagPointTop = new OneSidePoint
            {
                Point = new Point2D() { X = (LeftTop.X + RightBottom.X) / 2, Y = LeftTop.Y }
            };

            ControlPoint diagPointBottom = new OneSidePoint
            {
                Point = new Point2D() { X = (LeftTop.X + RightBottom.X) / 2, Y = RightBottom.Y }
            };

            ControlPoint moveControlPoint = new()
            {
                Point = new Point2D() { X = (LeftTop.X + RightBottom.X) / 2, Y = (LeftTop.Y + RightBottom.Y) / 2 },
                Type = "move"
            };

            controlPoints.Add(diagPointTopLeft);
            controlPoints.Add(diagPointTopRight);
            controlPoints.Add(diagPointBottomLeft);
            controlPoints.Add(diagPointBottomRight);
            controlPoints.Add(diagPointRight);
            controlPoints.Add(diagPointLeft);
            controlPoints.Add(diagPointBottom);
            controlPoints.Add(diagPointTop);
            controlPoints.Add(moveControlPoint);

            return controlPoints;
        }

        public IShape Clone()
        {
            return new Line2D();
        }

        override public PShape DeepCopy()
        {
            Line2D temp = new()
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
}

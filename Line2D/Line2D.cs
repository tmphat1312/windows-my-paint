using Contract;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Shapes
{
    public class Line2D : CShape, IShape
    {
        public DoubleCollection StrokeDash { get; set; }

        public SolidColorBrush Brush { get; set; }
        public string Name => "Line";
        public string Icon => "Assets/Shapes/line.png";

        public int Thickness { get; set; }

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
            Line line = new Line()
            {
                X1 = LeftTop.X,
                Y1 = LeftTop.Y,
                X2 = RightBottom.X,
                Y2 = RightBottom.Y,
                StrokeThickness = thickness,
                Stroke = brush,
                StrokeDashArray = dash
            };

            var width = Math.Abs(LeftTop.X - RightBottom.X);
            var height = Math.Abs(LeftTop.Y - RightBottom.Y);

            RotateTransform transform = new RotateTransform(this.RotateAngle);

            line.RenderTransform = transform;
            return line;
        }
        override public List<ControlPoint> GetControlPoints()
        {
            List<ControlPoint> controlPoints = new List<ControlPoint>();

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
            //ControlPoint diagPointLeft = new oneSidePoint();
            diagPointLeft.Point = new Point2D() { X = LeftTop.X, Y = (RightBottom.Y + LeftTop.Y) / 2 };


            ControlPoint diagPointTop = new OneSidePoint();
            //diagPointTop.setPoint((LeftTop.X + RightBottom.X) / 2, LeftTop.Y);
            diagPointTop.Point = new Point2D() { X = (LeftTop.X + RightBottom.X) / 2, Y = LeftTop.Y };

            ControlPoint diagPointBottom = new OneSidePoint();
            //diagPointBottom.setPoint((LeftTop.X + RightBottom.X) / 2, RightBottom.Y);
            diagPointBottom.Point = new Point2D() { X = (LeftTop.X + RightBottom.X) / 2, Y = RightBottom.Y };

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

            controlPoints.Add(moveControlPoint);

            return controlPoints;
        }

        public IShape Clone()
        {
            return new Line2D();
        }
        override public CShape DeepCopy()
        {
            Line2D temp = new Line2D();

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
}

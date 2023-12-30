using Contract;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Rectangle2D
{
    public class Rectangle2D : CShape, IShape
    {
        public SolidColorBrush Brush { get; set; }

        public DoubleCollection StrokeDash { get; set; }
        public string Icon => "Images/rectangle.png";
        public string Name => "Rectangle";

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

            RotateTransform transform = new RotateTransform(this.RotateAngle);
            transform.CenterX = width * 1.0 / 2;
            transform.CenterY = height * 1.0 / 2;

            rect.RenderTransform = transform;

            return rect;
        }

        public IShape Clone()
        {
            return new Rectangle2D();
        }

        override public CShape DeepCopy()
        {
            Rectangle2D temp = new Rectangle2D();

            temp.LeftTop = this.LeftTop.deepCopy();
            temp.RightBottom = this.RightBottom.deepCopy();
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

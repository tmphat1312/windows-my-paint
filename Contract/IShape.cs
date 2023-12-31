using System.Windows;
using System.Windows.Media;

namespace Contract;

public interface IShape
{
    public DoubleCollection? StrokeDash { get; set; }
    public SolidColorBrush Brush { get; set; }
    public int Thickness { get; set; }

    string Name { get; }
    string Icon { get; }

    void HandleStart(double x, double y);
    void HandleEnd(double x, double y);
    IShape Clone();
    UIElement Draw(SolidColorBrush brush, int thickness, DoubleCollection dash);
}

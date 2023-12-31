using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;

namespace Paint;

public class ThicknessOption
{
    public string Label { get; set; } = string.Empty;
    public int Value { get; set; }
    public FontWeight FontWeight { get; set; }
}

public class DashOption
{
    public string Label { get; set; } = string.Empty;
    public DoubleCollection Value { get; set; } = [];
}

public class MainViewModel
{
    public ObservableCollection<SolidColorBrush> ColorOptions { get; set; }
    public ObservableCollection<ThicknessOption> ThicknessOptions { get; set; }
    public ObservableCollection<DashOption> DashOptions { get; set; }

    public MainViewModel()
    {
        ColorOptions =
        [
            Brushes.Black,
            Brushes.Red,
            Brushes.Orange,
            Brushes.Yellow,
            Brushes.Blue,
            Brushes.Green,
            Brushes.Pink,
            Brushes.Brown
        ];

        ThicknessOptions =
        [
            new ThicknessOption { Label = "1px", Value = 1, FontWeight = FontWeights.Light },
            new ThicknessOption { Label = "2px", Value = 2, FontWeight = FontWeights.Normal },
            new ThicknessOption { Label = "3px", Value = 3, FontWeight = FontWeights.Medium },
            new ThicknessOption { Label = "5px", Value = 5, FontWeight = FontWeights.Bold }
        ];

        DashOptions =
        [
            new DashOption { Label = "__________", Value = [] },
            new DashOption { Label = "-----------", Value = [4, 2] },
            new DashOption { Label = "· · · · · · · ·", Value = [1, 2] },
            new DashOption { Label = "- · - · - · - ·", Value = [4, 2, 1, 2] },
            new DashOption { Label = "- · · - · · - ·", Value = [4, 2, 1, 2, 1, 2] },
        ];

        if (ColorOptions.Count == 0)
        {
            throw new Exception("Colors is empty");
        }

        if (ThicknessOptions.Count == 0)
        {
            throw new Exception("Thickness is empty");
        }

        if (DashOptions.Count == 0)
        {
            throw new Exception("Dash is empty");
        }
    }
}

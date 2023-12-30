using System.Collections.ObjectModel;
using System.Windows.Media;

namespace Paint;

public class MainViewModel
{
    public ObservableCollection<SolidColorBrush> Colors { get; set; }

    public MainViewModel()
    {
        Colors =
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
    }
}

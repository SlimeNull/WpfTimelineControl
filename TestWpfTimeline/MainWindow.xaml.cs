using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CommunityToolkit.Mvvm.ComponentModel;

namespace TestWpfTimeline
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    [ObservableObject]
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            DataContext = this;
            InitializeComponent();
        }

        [ObservableProperty]
        private double _timelineScale = 1;

        [ObservableProperty]
        private double _timeOffsetSeconds = 0;

        partial void OnTimeOffsetSecondsChanged(double value)
        {
            timeline.TimeOffset = TimeSpan.FromSeconds(value);
        }
    }
}
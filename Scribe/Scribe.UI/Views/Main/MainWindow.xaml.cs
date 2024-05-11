using System.Windows;

namespace Scribe.UI.Views.Main;

public partial class MainWindow : Window
{
    public MainWindow(MainViewModel mainViewModel)
    {
        InitializeComponent();
        DataContext = mainViewModel;
    }
}
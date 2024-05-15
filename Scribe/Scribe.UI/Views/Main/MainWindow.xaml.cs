using System.Windows;

namespace Scribe.UI.Views.Main;

public partial class MainWindow : Window
{
    private readonly MainViewModel _mainViewModel;
    
    public MainWindow(MainViewModel mainViewModel)
    {
        InitializeComponent();
        _mainViewModel = mainViewModel;
        DataContext = mainViewModel;
        InitViewModel();
    }
    
    private async void InitViewModel()
    {
        await _mainViewModel.Load();
    }
}
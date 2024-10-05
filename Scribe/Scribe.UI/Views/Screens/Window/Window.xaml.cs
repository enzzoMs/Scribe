using System.Windows;

namespace Scribe.UI.Views.Screens.Window;

public partial class Window : System.Windows.Window
{
    private readonly WindowViewModel _windowViewModel;
    
    public Window(WindowViewModel windowViewModel)
    {
        InitializeComponent();
        _windowViewModel = windowViewModel;
        DataContext = windowViewModel;
        InitViewModel();
    }
    
    private async void InitViewModel() => await _windowViewModel.Load();
}
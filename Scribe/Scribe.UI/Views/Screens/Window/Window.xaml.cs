using System.Windows.Input;

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

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        // Preventing focus when system key is pressed
        if (e.Key == Key.System)
        {
            e.Handled = true;
        }
    }

    private void OnActivated(object? sender, EventArgs e)
    {
        // A workaround to avoid focus on a control when the window is activated
        FocusableRectangle.Focus();
    }
}
using System.Windows.Controls;

namespace Scribe.UI.Views.Splash;

public partial class SplashControl : UserControl
{
    private readonly SplashViewModel _splashViewModel;

    public SplashControl()
    {
        InitializeComponent();
        _splashViewModel = new SplashViewModel();
        DataContext = _splashViewModel;
    }

    private void LogoSplashStoryboard_OnCompleted(object? sender, EventArgs e)
    {
        _splashViewModel.EndSplash();
    }
}
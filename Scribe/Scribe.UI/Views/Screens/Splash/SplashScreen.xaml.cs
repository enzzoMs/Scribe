using System.Windows.Controls;

namespace Scribe.UI.Views.Screens.Splash;

public partial class SplashScreen : UserControl
{
    public SplashScreen() => InitializeComponent();

    private void LogoPopUpStoryboard_OnCompleted(object? sender, EventArgs e)
    {
        (DataContext as SplashViewModel)?.FinishLogoAnimation();
    }

    private void LogoFadeOutStoryboard_OnCompleted(object? sender, EventArgs e)
    {
        (DataContext as SplashViewModel)?.FinishSplash();
    }
}
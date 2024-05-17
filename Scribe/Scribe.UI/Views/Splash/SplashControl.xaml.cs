using System.Windows.Controls;

namespace Scribe.UI.Views.Splash;

public partial class SplashControl : UserControl
{
    public SplashControl() => InitializeComponent();

    private void LogoPopUpStoryboard_OnCompleted(object? sender, EventArgs e)
    {
        (DataContext as SplashViewModel)?.FinishLogoAnimation();
    }

    private void LogoFadeOutStoryboard_OnCompleted(object? sender, EventArgs e)
    {
        (DataContext as SplashViewModel)?.FinishSplash();
    }
}
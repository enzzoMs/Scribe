using System.Windows.Controls;

namespace Scribe.UI.Views.Splash;

public partial class SplashControl : UserControl
{
    public SplashControl() => InitializeComponent();

    private void LogoSplashStoryboard_OnCompleted(object? sender, EventArgs e)
    {
        (DataContext as SplashViewModel)?.EndSplash();
    }
}
using Scribe.UI.Views.Splash;

namespace Scribe.UI.Views.Main;

public class MainViewModel(SplashViewModel splashViewModel) : BaseViewModel
{
    private BaseViewModel? _contentViewModel = splashViewModel;

    public BaseViewModel? ContentViewModel
    {
        get => _contentViewModel;
        set
        {
            _contentViewModel = value;
            RaisePropertyChanged();
        }
    }
}
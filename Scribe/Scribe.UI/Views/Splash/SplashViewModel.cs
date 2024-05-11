namespace Scribe.UI.Views.Splash;

public class SplashViewModel : BaseViewModel
{
    private bool _splashEnded;
    
    public bool SplashEnded
    {
        get => _splashEnded;
        private set
        {
            _splashEnded = value;
            RaisePropertyChanged();
        }
    }

    public void EndSplash() => SplashEnded = true;
}
using Scribe.Data.Model;
using Scribe.UI.Events;
using Scribe.UI.Views.Screens.Main;
using Scribe.UI.Views.Screens.Splash;

namespace Scribe.UI.Views.Screens.Window;

public class WindowViewModel : BaseViewModel
{
    private BaseViewModel _currentViewModel;
    private readonly MainViewModel _mainViewModel;
    
    public WindowViewModel(
        IEventAggregator eventAggregator, SplashViewModel splashViewModel, MainViewModel mainViewModel)
    {
        _currentViewModel = splashViewModel;
        _mainViewModel = mainViewModel;
        
        eventAggregator.Subscribe<FoldersLoadedEvent>(this, eventData =>
        {
            NavigateToEditorScreen(eventData.Folders);
            eventAggregator.Unsubscribe<FoldersLoadedEvent>(this);
        });
    }
    
    public BaseViewModel CurrentViewModel
    {
        get => _currentViewModel;
        private set
        {
            _currentViewModel = value;
            RaisePropertyChanged();
        }
    }
    
    public override Task Load() => _currentViewModel.Load();

    private void NavigateToEditorScreen(IEnumerable<Folder> folders)
    {
        _mainViewModel.LoadFolders(folders);
        CurrentViewModel = _mainViewModel;
    }
}
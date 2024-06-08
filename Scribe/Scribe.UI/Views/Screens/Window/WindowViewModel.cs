using Scribe.Data.Model;
using Scribe.UI.Events;
using Scribe.UI.Views.Screens.Main;
using Scribe.UI.Views.Screens.Splash;

namespace Scribe.UI.Views.Screens.Window;

public class WindowViewModel : BaseViewModel
{
    private BaseViewModel _currentViewModel;
    private readonly MainViewModel _mainViewModel;

    private readonly Action<FoldersLoadedEvent> _onFoldersLoaded;
    
    public WindowViewModel(
        IEventAggregator eventAggregator, SplashViewModel splashViewModel, MainViewModel mainViewModel)
    {
        _currentViewModel = splashViewModel;
        _mainViewModel = mainViewModel;
        
        _onFoldersLoaded = eventData =>
        {
            NavigateToEditorScreen(eventData.Folders);
            eventAggregator.Unsubscribe(_onFoldersLoaded!);
        };
        
        eventAggregator.Subscribe(_onFoldersLoaded);
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
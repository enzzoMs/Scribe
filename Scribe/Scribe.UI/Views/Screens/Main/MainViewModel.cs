using Scribe.Data.Model;
using Scribe.UI.Events;
using Scribe.UI.Views.Screens.Editor;
using Scribe.UI.Views.Screens.Splash;

namespace Scribe.UI.Views.Screens.Main;

public class MainViewModel : BaseViewModel
{
    private BaseViewModel _currentViewModel;
    private readonly EditorViewModel _editorViewModel;

    private readonly Action<FoldersLoadedEvent> _onFoldersLoaded;
    
    public MainViewModel(
        IEventAggregator eventAggregator, SplashViewModel splashViewModel, EditorViewModel editorViewModel)
    {
        _currentViewModel = splashViewModel;
        _editorViewModel = editorViewModel;
        
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
        _editorViewModel.LoadFolders(folders);
        CurrentViewModel = _editorViewModel;
    }
}
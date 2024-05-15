using Scribe.Data.Model;
using Scribe.UI.Events;
using Scribe.UI.Views.Editor;
using Scribe.UI.Views.Splash;

namespace Scribe.UI.Views.Main;

public class MainViewModel : BaseViewModel
{
    private BaseViewModel _contentViewModel;
    private readonly EditorViewModel _editorViewModel;

    private readonly Action<FoldersLoadedEvent> _onFoldersLoaded;
    
    public BaseViewModel ContentViewModel
    {
        get => _contentViewModel;
        private set
        {
            _contentViewModel = value;
            RaisePropertyChanged();
        }
    }

    public MainViewModel(
        IEventAggregator eventAggregator, SplashViewModel splashViewModel, EditorViewModel editorViewModel
    ) {
        _contentViewModel = splashViewModel;
        _editorViewModel = editorViewModel;
        
        _onFoldersLoaded = e =>
        {
            NavigateToEditorScreen(e.Folders);
            eventAggregator.Unsubscribe(_onFoldersLoaded!);
        };
        
        eventAggregator.Subscribe(_onFoldersLoaded);
    }
    
    public override Task Load() => _contentViewModel.Load();

    private void NavigateToEditorScreen(List<Folder> folders)
    {
        ContentViewModel = _editorViewModel;
    }
}
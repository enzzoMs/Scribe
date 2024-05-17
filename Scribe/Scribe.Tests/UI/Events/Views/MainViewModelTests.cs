using NSubstitute;
using Scribe.Data.Model;
using Scribe.Data.Repositories;
using Scribe.UI.Events;
using Scribe.UI.Views.Editor;
using Scribe.UI.Views.Folders;
using Scribe.UI.Views.Main;
using Scribe.UI.Views.Splash;

namespace Scribe.Tests.UI.Events.Views;

public class MainViewModelTests
{
    private readonly EventAggregator _eventAggregator = new();
    private readonly SplashViewModel _splashViewModel;
    private readonly MainViewModel _mainViewModel;

    public MainViewModelTests()
    {
        var foldersRepository = Substitute.For<IRepository<Folder>>();
        foldersRepository.GetAll().Returns([]);
        
        var editorViewModel = new EditorViewModel(new FoldersViewModel(foldersRepository));
        _splashViewModel = new SplashViewModel(_eventAggregator, foldersRepository);
        _mainViewModel = new MainViewModel(_eventAggregator, _splashViewModel, editorViewModel);
    }
    
    [Fact]
    public void SplashIsFirstViewModel() => Assert.IsType<SplashViewModel>(_mainViewModel.ContentViewModel);

    [Fact]
    public async Task NavigateToEditorWhenSplashFinishes()
    {
        await _splashViewModel.Load();
        _splashViewModel.FinishLogoAnimation();
        _splashViewModel.FinishSplash();
        
        Assert.IsType<EditorViewModel>(_mainViewModel.ContentViewModel);
    }
}
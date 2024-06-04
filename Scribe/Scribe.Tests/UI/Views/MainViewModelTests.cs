using NSubstitute;
using Scribe.Data.Configurations;
using Scribe.Data.Model;
using Scribe.Data.Repositories;
using Scribe.UI.Events;
using Scribe.UI.Resources;
using Scribe.UI.Views.Screens.Editor;
using Scribe.UI.Views.Screens.Main;
using Scribe.UI.Views.Screens.Splash;
using Scribe.UI.Views.Sections.Configurations;
using Scribe.UI.Views.Sections.FolderDetails;
using Scribe.UI.Views.Sections.Navigation;

namespace Scribe.Tests.UI.Views;

public class MainViewModelTests
{
    private readonly EventAggregator _eventAggregator = new();
    private readonly SplashViewModel _splashViewModel;
    private readonly MainViewModel _mainViewModel;

    public MainViewModelTests()
    {
        var foldersRepository = Substitute.For<IRepository<Folder>>();
        foldersRepository.GetAll().Returns([]);

        var configurationsRepository = Substitute.For<IConfigurationsRepository>();
        configurationsRepository.GetAllConfigurations().Returns(
            new AppConfigurations(ThemeConfiguration.Light, LanguageConfiguration.EnUs, 1.0)    
        );

        var documentsRepository = Substitute.For<IRepository<Document>>();
        
        var resourcesManager = Substitute.For<IResourceManager>();

        var configurationsViewModel = new ConfigurationsViewModel(configurationsRepository, resourcesManager);
        var navigationViewModel = new NavigationViewModel(_eventAggregator, foldersRepository, configurationsViewModel);
        var folderDetailsViewModel = new FolderDetailsViewModel(_eventAggregator, foldersRepository, documentsRepository);
        var editorViewModel = new EditorViewModel(navigationViewModel, folderDetailsViewModel);
        
        _splashViewModel = new SplashViewModel(_eventAggregator, foldersRepository);
        _mainViewModel = new MainViewModel(_eventAggregator, _splashViewModel, editorViewModel);
    }
    
    [Fact]
    public void SplashIsFirstViewModel() => Assert.IsType<SplashViewModel>(_mainViewModel.CurrentViewModel);

    [Fact]
    public async Task NavigatesToEditorWhenSplashFinishes()
    {
        await _splashViewModel.Load();
        _splashViewModel.FinishLogoAnimation();
        _splashViewModel.FinishSplash();
        
        Assert.IsType<EditorViewModel>(_mainViewModel.CurrentViewModel);
    }
}
using NSubstitute;
using Scribe.Data.Configurations;
using Scribe.Data.Model;
using Scribe.Data.Repositories;
using Scribe.UI.Events;
using Scribe.UI.Resources;
using Scribe.UI.Views.Screens.Main;
using Scribe.UI.Views.Screens.Splash;
using Scribe.UI.Views.Screens.Window;
using Scribe.UI.Views.Sections.Configurations;
using Scribe.UI.Views.Sections.Editor;
using Scribe.UI.Views.Sections.FolderDetails;
using Scribe.UI.Views.Sections.Navigation;

namespace Scribe.Tests.UI.Views;

public class WindowViewModelTests
{
    private readonly EventAggregator _eventAggregator = new();
    private readonly SplashViewModel _splashViewModel;
    private readonly WindowViewModel _windowViewModel;

    public WindowViewModelTests()
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
        var editorViewModel = new EditorViewModel(_eventAggregator);
        
        var mainViewModel = new MainViewModel(navigationViewModel, folderDetailsViewModel, editorViewModel);
        
        _splashViewModel = new SplashViewModel(_eventAggregator, foldersRepository);
        _windowViewModel = new WindowViewModel(_eventAggregator, _splashViewModel, mainViewModel);
    }
    
    [Fact]
    public void SplashIsFirstViewModel() => Assert.IsType<SplashViewModel>(_windowViewModel.CurrentViewModel);

    [Fact]
    public async Task NavigatesToEditorWhenSplashFinishes()
    {
        await _splashViewModel.Load();
        _splashViewModel.FinishLogoAnimation();
        _splashViewModel.FinishSplash();
        
        Assert.IsType<MainViewModel>(_windowViewModel.CurrentViewModel);
    }
}
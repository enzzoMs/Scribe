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
using Scribe.UI.Views.Sections.Documents;
using Scribe.UI.Views.Sections.Editor;
using Scribe.UI.Views.Sections.FolderDetails;
using Scribe.UI.Views.Sections.Navigation;
using Scribe.UI.Views.Sections.Tags;

namespace Scribe.Tests.UI.Views;

public class WindowViewModelTests
{
    private readonly EventAggregator _eventAggregator = new();
    private readonly SplashViewModel _splashViewModel;
    private readonly WindowViewModel _windowViewModel;

    public WindowViewModelTests()
    {
        var foldersRepositoryMock = Substitute.For<IRepository<Folder>>();
        foldersRepositoryMock.GetAll().Returns([]);

        var configurationsRepositoryMock = Substitute.For<IConfigurationsRepository>();
        configurationsRepositoryMock.GetAllConfigurations().Returns(
            new AppConfigurations(ThemeConfiguration.Light, LanguageConfiguration.EnUs, 1.0)    
        );

        var documentsRepositoryMock = Substitute.For<IRepository<Document>>();
        var tagsRepositoryMock = Substitute.For<IRepository<Tag>>();
        var resourcesManagerMock = Substitute.For<IResourceManager>();

        var tagsViewModel = new TagsViewModel(_eventAggregator);
        var configurationsViewModel = new ConfigurationsViewModel(configurationsRepositoryMock, resourcesManagerMock);
        var navigationViewModel = new NavigationViewModel(_eventAggregator, foldersRepositoryMock, tagsRepositoryMock, configurationsViewModel);
        var folderDetailsViewModel = new FolderDetailsViewModel(_eventAggregator, foldersRepositoryMock, documentsRepositoryMock);
        var editorViewModel = new EditorViewModel(_eventAggregator, documentsRepositoryMock, configurationsRepositoryMock);
        var documentsViewModel = new DocumentsViewModel(_eventAggregator, documentsRepositoryMock);
        var mainViewModel = new MainViewModel(
            navigationViewModel, folderDetailsViewModel, documentsViewModel, tagsViewModel, editorViewModel
        );
        
        _splashViewModel = new SplashViewModel(_eventAggregator, foldersRepositoryMock);
        _windowViewModel = new WindowViewModel(_eventAggregator, _splashViewModel, mainViewModel);
    }
    
    [Fact]
    public void SplashIsFirstViewModel() => Assert.IsType<SplashViewModel>(_windowViewModel.CurrentViewModel);

    [Fact]
    public async Task NavigatesToEditor_When_SplashFinishes()
    {
        await _splashViewModel.Load();
        _splashViewModel.FinishLogoAnimation();
        _splashViewModel.FinishSplash();
        
        Assert.IsType<MainViewModel>(_windowViewModel.CurrentViewModel);
    }
}
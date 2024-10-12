using NSubstitute;
using Scribe.Data.Configurations;
using Scribe.Data.Model;
using Scribe.Data.Repositories;
using Scribe.UI.Events;
using Scribe.UI.Helpers;
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
    private readonly NavigationViewModel _navigationViewModel;
    private readonly SplashViewModel _splashViewModel;
    private readonly WindowViewModel _windowViewModel;
    private readonly IRepository<Folder> _foldersRepositoryMock; 

    public WindowViewModelTests()
    {
        _foldersRepositoryMock = Substitute.For<IRepository<Folder>>();
        _foldersRepositoryMock.GetAll().Returns([]);

        var configurationsRepositoryMock = Substitute.For<IConfigurationsRepository>();
        configurationsRepositoryMock.GetAllConfigurations().Returns(
            new AppConfigurations(ThemeConfiguration.Light, LanguageConfiguration.EnUs, 1.0)    
        );

        var documentsRepositoryMock = Substitute.For<IRepository<Document>>();
        var tagsRepositoryMock = Substitute.For<IRepository<Tag>>();
        var resourcesManagerMock = Substitute.For<IResourceManager>();
        var pdfHelperMock = Substitute.For<IPdfHelper>();

        var tagsViewModel = new TagsViewModel(_eventAggregator);
        var configurationsViewModel = new ConfigurationsViewModel(configurationsRepositoryMock, resourcesManagerMock);
        var folderDetailsViewModel = new FolderDetailsViewModel(_eventAggregator, _foldersRepositoryMock, documentsRepositoryMock);
        var editorViewModel = new EditorViewModel(_eventAggregator, documentsRepositoryMock, configurationsRepositoryMock, pdfHelperMock);
        var documentsViewModel = new DocumentsViewModel(_eventAggregator, documentsRepositoryMock);

        _navigationViewModel = new NavigationViewModel(_eventAggregator, _foldersRepositoryMock, tagsRepositoryMock, configurationsViewModel);

        var mainViewModel = new MainViewModel(
            _navigationViewModel, folderDetailsViewModel, documentsViewModel, tagsViewModel, editorViewModel
        );
        
        _splashViewModel = new SplashViewModel(_eventAggregator, _foldersRepositoryMock);
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

    [Fact]
    public async Task FoldersAreLoaded_IntoNavigation_When_SplashFinishes()
    {
        var folders = new List<Folder> { new("A", 0), new("B", 1), new("C", 2) };

        _foldersRepositoryMock.GetAll().Returns(folders);
        
        await _splashViewModel.Load();
        _splashViewModel.FinishLogoAnimation();
        _splashViewModel.FinishSplash();
        
        Assert.Equal(folders, _navigationViewModel.CurrentFolders);
    }
}
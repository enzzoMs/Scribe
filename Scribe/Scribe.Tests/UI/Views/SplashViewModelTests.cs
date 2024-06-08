using NSubstitute;
using Scribe.Data.Model;
using Scribe.Data.Repositories;
using Scribe.UI.Events;
using Scribe.UI.Views.Screens.Splash;

namespace Scribe.Tests.UI.Views;

public class SplashViewModelTests
{
    private readonly EventAggregator _eventAggregator = new();
    private readonly IRepository<Folder> _foldersRepositoryMock = Substitute.For<IRepository<Folder>>();
    private readonly SplashViewModel _splashViewModel;

    public SplashViewModelTests() => _splashViewModel = new SplashViewModel(_eventAggregator, _foldersRepositoryMock);
    
    [Fact]
    public async Task DataIsLoadedAndReceived()
    {
        List<Folder> expectedFolders = [new Folder("", 0), new Folder("", 3)];
        List<Folder>? receivedFolders = null;
        
        _foldersRepositoryMock.GetAll().Returns(expectedFolders);
        _eventAggregator.Subscribe<FoldersLoadedEvent>(this, e => receivedFolders = e.Folders);

        await _splashViewModel.Load();
        _splashViewModel.FinishLogoAnimation();
        _splashViewModel.FinishSplash();

        Assert.NotNull(receivedFolders);
        Assert.Equal(expectedFolders, receivedFolders);
    }

    [Fact]
    public async Task DataIsLoadedOnlyOnce()
    {
        List<Folder> expectedFolders = [];
        List<Folder>? receivedFolders = null;
        
        _foldersRepositoryMock.GetAll().Returns(expectedFolders);
        _eventAggregator.Subscribe<FoldersLoadedEvent>(this, e => receivedFolders = e.Folders);
        
        await _splashViewModel.Load();
        _splashViewModel.FinishLogoAnimation();
        _splashViewModel.FinishSplash();

        Assert.Equal(expectedFolders, receivedFolders);

        _foldersRepositoryMock.GetAll().Returns([new Folder("", 0)]);

        await _splashViewModel.Load();
        
        Assert.Equal(expectedFolders, receivedFolders);
    }
    
    [Fact]
    public async Task LoadedEventIsPublishedOnlyOnce()
    {
        var publishedEvents = 0;
        
        _foldersRepositoryMock.GetAll().Returns([]);
        _eventAggregator.Subscribe<FoldersLoadedEvent>(this, _ => publishedEvents++);
        
        await _splashViewModel.Load();
        _splashViewModel.FinishLogoAnimation();
        _splashViewModel.FinishSplash();
        
        _splashViewModel.FinishSplash();

        Assert.Equal(1, publishedEvents);
    }
}
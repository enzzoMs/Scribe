using NSubstitute;
using Scribe.Data.Configurations;
using Scribe.Data.Model;
using Scribe.Data.Repositories;
using Scribe.UI.Events;
using Scribe.UI.Resources;
using Scribe.UI.Views.Sections.Configurations;
using Scribe.UI.Views.Sections.Navigation;

namespace Scribe.Tests.UI.Views;

public class NavigationViewModelTests
{
    private readonly NavigationViewModel _navigationViewModel;
    private readonly EventAggregator _eventAggregator = new();
    private readonly IRepository<Folder> _folderRepository = Substitute.For<IRepository<Folder>>();
    
    public NavigationViewModelTests()
    {
        var configurationsRepository = Substitute.For<IConfigurationsRepository>();
        configurationsRepository.GetAllConfigurations().Returns(new AppConfigurations(
            ThemeConfiguration.Light, LanguageConfiguration.EnUs, 1.0
        ));
        
        var resourceManager = Substitute.For<IResourceManager>();

        var configurationsViewModel = new ConfigurationsViewModel(configurationsRepository, resourceManager);

        _navigationViewModel = new NavigationViewModel(_eventAggregator, _folderRepository, configurationsViewModel);
    }

    [Fact]
    public void CanLoadFolders()
    {
        List<Folder> folders = [new Folder("", 0), new Folder("", 1)];
        
        _navigationViewModel.LoadFolders(folders);
        
        Assert.Equivalent(_navigationViewModel.CurrentFolders, folders);
    }

    [Fact]
    public void LoadedFoldersAreSortedByNavIndex()
    {
        List<Folder> folders = [
            new Folder("FolderIndex10", navigationIndex: 10), 
            new Folder("FolderIndex0", navigationIndex: 0), 
            new Folder("FolderIndex5", navigationIndex: 5)
        ];
        
        _navigationViewModel.LoadFolders(folders);
        
        Assert.Equal("FolderIndex0", _navigationViewModel.CurrentFolders[0].Name);
        Assert.Equal("FolderIndex5", _navigationViewModel.CurrentFolders[1].Name);
        Assert.Equal("FolderIndex10", _navigationViewModel.CurrentFolders[2].Name);
    }

    [Fact]
    public void SearchFoldersFilter_Setter_FiltersFolders()
    {
        List<Folder> folders = [
            new Folder("No", 0), new Folder("No", 1), new Folder("Yes", 2)
        ];
        
        _navigationViewModel.LoadFolders(folders);
        _navigationViewModel.DelayFoldersSearch = false;
        _navigationViewModel.SearchFoldersFilter = "Yes";
        
        Assert.Single(_navigationViewModel.CurrentFolders);
        Assert.Equal("Yes", _navigationViewModel.CurrentFolders[0].Name);
    }

    [Fact]
    public void FolderSelectionRaisesEvent()
    {
        var folderEventHandled = false;
        
        _eventAggregator.Subscribe<FolderSelectedEvent>(this, _ => folderEventHandled = true);

        _navigationViewModel.SelectedFolder = new Folder("", 0);
        
        Assert.True(folderEventHandled);
    }

    [Fact]
    public void CollapseNavigationCommand_Sets_IsNavigationCollapsed()
    {
        _navigationViewModel.IsNavigationCollapsed = false;
        
        _navigationViewModel.CollapseNavigationCommand.Execute(true);
        
        Assert.True(_navigationViewModel.IsNavigationCollapsed);
    }

    [Fact]
    public void CreateFolderCommand_AddsNewFolder()
    {
        var newFolder = new Folder("NewFolder", 0);

        _folderRepository.Add(Arg.Any<Folder>()).Returns(newFolder);
        
        _navigationViewModel.CreateFolderCommand.Execute(null);

        _folderRepository.Received(1);
        Assert.Single(_navigationViewModel.CurrentFolders);
        Assert.Equal(newFolder, _navigationViewModel.CurrentFolders[0]);
    }

    [Fact]
    public void FolderDeletedEvent_RemovesDeletedFolder()
    {
        List<Folder> folders = [new Folder("", 0), new Folder("", 1)];
        var deletedFolder = folders[0];
        
        _navigationViewModel.LoadFolders(folders);
        
        _eventAggregator.Publish(new FolderDeletedEvent(deletedFolder));

        Assert.Single(_navigationViewModel.CurrentFolders);
        Assert.DoesNotContain(deletedFolder, _navigationViewModel.CurrentFolders);
    }

    [Fact]
    public void FolderDeletion_Updates_NavigationIndexes()
    {
        List<Folder> folders = [
            new Folder("", navigationIndex: 0), 
            new Folder("", navigationIndex: 1), 
            new Folder("", navigationIndex: 2)
        ];
        var deletedFolder = folders[0];
        
        _navigationViewModel.LoadFolders(folders);
        
        _eventAggregator.Publish(new FolderDeletedEvent(deletedFolder));

        _folderRepository.Received(1);
        Assert.Equal(0, _navigationViewModel.CurrentFolders[0].NavigationIndex);
        Assert.Equal(1, _navigationViewModel.CurrentFolders[1].NavigationIndex);
    }

    [Fact]
    public void FolderPositionUpdatedEvent_SwapsFolderPositions()
    {
        var folderA = new Folder("A", navigationIndex: 0);
        var folderB = new Folder("B", navigationIndex: 1);

        var oldIndex = folderA.NavigationIndex;
        var newIndex = folderA.NavigationIndex + 1;
        
        _navigationViewModel.LoadFolders([folderA, folderB]);
        
        _eventAggregator.Publish(new FolderPositionUpdatedEvent(folderA.Id, oldIndex, newIndex));
        
        Assert.Equal(folderB, _navigationViewModel.CurrentFolders[oldIndex]);
        Assert.Equal(folderA, _navigationViewModel.CurrentFolders[newIndex]);

        Assert.Equal(newIndex, folderA.NavigationIndex);
        Assert.Equal(oldIndex, folderB.NavigationIndex);
        
        _folderRepository.Received(2);
    }
    
    [Fact]
    public void UpdatedEventForSelectedFolder_RaisesPropertyChanged()
    {
        var folder = new Folder("", 0);

        var eventRaised = false;
        
        _navigationViewModel.LoadFolders([folder]);
        _navigationViewModel.SelectedFolder = folder;

        _navigationViewModel.PropertyChanged += (_, _) => eventRaised = true;
        _eventAggregator.Publish(new FolderUpdatedEvent(folder.Id));
        
        Assert.True(eventRaised);
    }
}
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
    private readonly IRepository<Folder> _folderRepositoryMock = Substitute.For<IRepository<Folder>>();
    private readonly IRepository<Tag> _tagRepositoryMock = Substitute.For<IRepository<Tag>>();

    public NavigationViewModelTests()
    {
        var configurationsRepositoryMock = Substitute.For<IConfigurationsRepository>();
        configurationsRepositoryMock.GetAllConfigurations().Returns(new AppConfigurations(
            ThemeConfiguration.Light, LanguageConfiguration.EnUs, 1.0
        ));

        var resourceManagerMock = Substitute.For<IResourceManager>();
        
        var configurationsViewModel = new ConfigurationsViewModel(configurationsRepositoryMock, resourceManagerMock);

        _navigationViewModel = new NavigationViewModel(
            _eventAggregator, _folderRepositoryMock, _tagRepositoryMock, configurationsViewModel
        );
    }

    [Fact]
    public void CanLoadFolders()
    {
        List<Folder> folders = [new Folder("", 0), new Folder("", 1)];
        
        _navigationViewModel.LoadFolders(folders);
        
        Assert.Equivalent(_navigationViewModel.CurrentFolders, folders);
    }

    [Fact]
    public void LoadedFolders_Are_SortedByNavIndex()
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
            new Folder("No", 0), 
            new Folder("No", 1), 
            new Folder("Yes", 2)
        ];
        
        _navigationViewModel.LoadFolders(folders);
        _navigationViewModel.DelayFoldersSearch = false;
        _navigationViewModel.SearchFoldersFilter = "Yes";
        
        Assert.Single(_navigationViewModel.CurrentFolders);
        Assert.Equal("Yes", _navigationViewModel.CurrentFolders[0].Name);
    }

    [Fact]
    public void FolderSelection_Raises_FolderSelectedEvent()
    {
        FolderSelectedEvent? folderEvent = null;
        
        _eventAggregator.Subscribe<FolderSelectedEvent>(this, e => folderEvent = e);

        var selectedFolder = new Folder("", 0);
        _navigationViewModel.SelectedFolder = selectedFolder;

        Assert.NotNull(folderEvent);
        Assert.Equal(selectedFolder, folderEvent.Folder);
    }

    [Fact]
    public void CollapseNavigationCommand_Sets_IsNavigationCollapsed()
    {
        _navigationViewModel.IsNavigationCollapsed = false;
        
        _navigationViewModel.CollapseNavigationCommand.Execute(true);
        Assert.True(_navigationViewModel.IsNavigationCollapsed);
        
        _navigationViewModel.CollapseNavigationCommand.Execute(false);
        Assert.False(_navigationViewModel.IsNavigationCollapsed);
    }

    [Fact]
    public void CreateFolderCommand_AddsNewFolder()
    {
        var newFolder = new Folder("NewFolder", 0);

        _folderRepositoryMock.Add(Arg.Any<Folder>()).Returns([newFolder]);
        
        _navigationViewModel.CreateFolderCommand.Execute(null);

        _folderRepositoryMock.Received(1).Add(Arg.Any<Folder>());
        Assert.Single(_navigationViewModel.CurrentFolders);
        Assert.Equal(newFolder, _navigationViewModel.CurrentFolders[0]);
    }
    
    [Fact]
    public void CreateFolderCommand_ShowsAllFolders()
    {
        List<Folder> folders = [
            new Folder("No", navigationIndex: 0), 
            new Folder("No", navigationIndex: 1), 
            new Folder("Yes", navigationIndex: 2)
        ];
        
        _navigationViewModel.LoadFolders(folders);
        _navigationViewModel.DelayFoldersSearch = false;
        _navigationViewModel.SearchFoldersFilter = "Yes";
        
        var newFolder = new Folder("NewFolder", 0);
        
        _folderRepositoryMock.Add(Arg.Any<Folder>()).Returns([newFolder]);
        _navigationViewModel.CreateFolderCommand.Execute(null);

        Assert.Empty(_navigationViewModel.SearchFoldersFilter);
        Assert.Equal(4, _navigationViewModel.CurrentFolders.Count);
    }

    [Fact]
    public void FolderDeletedEvent_RemovesFolder()
    {
        List<Folder> folders = [new Folder("", 0), new Folder("", 1)];
        _navigationViewModel.LoadFolders(folders);
        
        var deletedFolder = folders[0];
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
        _navigationViewModel.LoadFolders(folders);
        
        var deletedFolder = folders[0];
        var remainingFolders = folders[1..];
        _eventAggregator.Publish(new FolderDeletedEvent(deletedFolder));

        _folderRepositoryMock.Received(1).Update(remainingFolders.ToArray());
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
        
        _folderRepositoryMock.Received(1).Update(folderB, folderA);
    }
    
    [Fact]
    public void UpdatedEvent_ForSelectedFolder_RaisesPropertyChanged()
    {
        var folder = new Folder("", 0);

        var eventRaised = false;
        
        _navigationViewModel.LoadFolders([folder]);
        _navigationViewModel.SelectedFolder = folder;

        _navigationViewModel.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(_navigationViewModel.SelectedFolder))
            {
                eventRaised = true;
            }
        };
        _eventAggregator.Publish(new FolderUpdatedEvent(folder.Id));
        
        Assert.True(eventRaised);
    }
    
    [Fact]
    public void FolderDeletedOrUpdatedEvent_ShowsAllFolders()
    {
        List<Folder> folders = [
            new Folder("Yes", navigationIndex: 0), 
            new Folder("No", navigationIndex: 1), 
            new Folder("No", navigationIndex: 2)
        ];
        
        _navigationViewModel.LoadFolders(folders);
        _navigationViewModel.DelayFoldersSearch = false;
        _navigationViewModel.SearchFoldersFilter = "Yes";
        
        _eventAggregator.Publish(new FolderDeletedEvent(folders[0]));

        Assert.Empty(_navigationViewModel.SearchFoldersFilter);
        Assert.Equal(2, _navigationViewModel.CurrentFolders.Count);
        
        _navigationViewModel.DelayFoldersSearch = false;
        _navigationViewModel.SearchFoldersFilter = "Yes";
        
        _eventAggregator.Publish(new FolderPositionUpdatedEvent(0, 0, 1));

        Assert.Empty(_navigationViewModel.SearchFoldersFilter);
        Assert.Equal(2, _navigationViewModel.CurrentFolders.Count);
    }
    
    [Fact]
    public void TagAddedEvent_UpdatesRelatedFolder()
    {
        var folder = new Folder("", navigationIndex: 0);
        _navigationViewModel.LoadFolders([folder]);

        var newTag = new Tag("", folderId: 0);
        _eventAggregator.Publish(new TagAddedEvent(newTag));

        Assert.Single(folder.Tags);
        Assert.Equivalent(newTag, folder.Tags.First());
    }

    [Fact]
    public void TagAddedEvent_AddsTagToRepository()
    {
        var folder = new Folder("", navigationIndex: 0);
        _navigationViewModel.LoadFolders([folder]);

        var newTag = new Tag("", folderId: 0);
        _eventAggregator.Publish(new TagAddedEvent(newTag));

        _tagRepositoryMock.Received(1).Add(newTag);
    }
    
    [Fact]
    public void TagAddedEvent_IgnoresFolder_IfTagAlreadyExists()
    {
        var folder = new Folder("", navigationIndex: 0);
        var newTag = new Tag("", folderId: 0);
        folder.Tags.Add(newTag);
        
        _navigationViewModel.LoadFolders([folder]);
        _eventAggregator.Publish(new TagAddedEvent(newTag));

        _tagRepositoryMock.Received(0);
        Assert.Single(folder.Tags);
        Assert.Equivalent(newTag, folder.Tags.First());
    }
    
    [Fact]
    public void TagRemovedEvent_UpdatesRelatedFolder()
    {
        var folder = new Folder("", navigationIndex: 0);
        var tag = new Tag("", folderId: 0);
        folder.Tags.Add(tag);

        _navigationViewModel.LoadFolders([folder]);
        _eventAggregator.Publish(new TagRemovedEvent(tag));

        Assert.Empty(folder.Tags);
    }

    [Fact]
    public void TagRemovedEvent_RemovesTagFromRepository()
    {
        var folder = new Folder("", navigationIndex: 0);
        var tag = new Tag("", folderId: 0);
        folder.Tags.Add(tag);
        
        _navigationViewModel.LoadFolders([folder]);
        _eventAggregator.Publish(new TagRemovedEvent(tag));

        _tagRepositoryMock.Received(1).Delete(tag);
    }
    
    [Fact]
    public void TagRemovedEvent_IgnoresFolder_IfTagIsStillUsed()
    {
        var tag = new Tag("", folderId: 0);

        var document = new Document(0, DateTime.Now, DateTime.Now);
        document.Tags.Add(tag);
        
        var folder = new Folder("", navigationIndex: 0);
        folder.Documents.Add(document);
        folder.Tags.Add(tag);
        
        _navigationViewModel.LoadFolders([folder]);
        _eventAggregator.Publish(new TagRemovedEvent(tag));

        _tagRepositoryMock.Received(0);
        Assert.Single(folder.Tags);
        Assert.Equivalent(tag, folder.Tags.First());
    }
}
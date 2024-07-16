using NSubstitute;
using Scribe.Data.Model;
using Scribe.Data.Repositories;
using Scribe.UI.Events;
using Scribe.UI.Views.Sections.FolderDetails;

namespace Scribe.Tests.UI.Views;

public class FolderDetailsViewModelTests
{
    private readonly EventAggregator _eventAggregator = new();
    private readonly IRepository<Folder> _foldersRepositoryMock = Substitute.For<IRepository<Folder>>();
    private readonly FolderDetailsViewModel _folderDetailsViewModel;

    public FolderDetailsViewModelTests() =>
        _folderDetailsViewModel = new FolderDetailsViewModel(_eventAggregator, _foldersRepositoryMock);

    [Fact]
    public void FolderSelectedEvent_Sets_FolderProperty()
    {
        var folder = new Folder("", 0);
        
        _eventAggregator.Publish(new FolderSelectedEvent(folder));
        
        Assert.Equal(folder, _folderDetailsViewModel.Folder);
    }
    
    [Fact]
    public void FolderNavigationPosition_ReflectsFolderProperty()
    {
        const int navIndex = 99;
        var folder = new Folder("", navIndex);
        
        _eventAggregator.Publish(new FolderSelectedEvent(folder));
        
        Assert.Equal(navIndex + 1, _folderDetailsViewModel.FolderNavigationPosition);
    }

    [Fact]
    public void FolderNavigationPosition_Setter_PublishesFolderUpdatedEvent()
    {
        const int oldNavIndex = 0;
        const int newNavIndex = 99;
        
        var folder = new Folder("", oldNavIndex);
        _eventAggregator.Publish(new FolderSelectedEvent(folder));

        FolderUpdatedEvent? folderUpdatedEvent = null;
        _eventAggregator.Subscribe<FolderUpdatedEvent>(this, e => folderUpdatedEvent = e);
        
        _folderDetailsViewModel.FolderNavigationPosition = newNavIndex + 1;

        Assert.NotNull(folderUpdatedEvent);
        Assert.IsType<FolderPositionUpdatedEvent>(folderUpdatedEvent);
        Assert.Equal(folder.Id, folderUpdatedEvent.UpdatedFolderId);
        Assert.Equal(oldNavIndex, ((FolderPositionUpdatedEvent) folderUpdatedEvent).OldIndex);
        Assert.Equal(newNavIndex, ((FolderPositionUpdatedEvent) folderUpdatedEvent).NewIndex);
    }
    
    [Fact]
    public void EnterEditModeCommand_Sets_OnEditMode()
    {
        _folderDetailsViewModel.InEditMode = false;

        _folderDetailsViewModel.EnterEditModeCommand.Execute(null);
        
        Assert.True(_folderDetailsViewModel.InEditMode);
    }

    [Fact]
    public void UpdateFolderNameCommand_Updates_FolderName()
    {
        var folder = new Folder("", 0);
        _eventAggregator.Publish(new FolderSelectedEvent(folder));

        const string newFolderName = "Name";
        
        _folderDetailsViewModel.UpdateFolderNameCommand.Execute(newFolderName);

        _foldersRepositoryMock.Received(1).Update(Arg.Any<Folder[]>());
        Assert.Equal(newFolderName, _folderDetailsViewModel.Folder?.Name);
    }
    
    [Fact]
    public void UpdateFolderNameCommand_Publishes_FolderUpdatedEvent()
    {
        var folder = new Folder("", 0);
        _eventAggregator.Publish(new FolderSelectedEvent(folder));
        
        FolderUpdatedEvent? folderUpdatedEvent = null;
        _eventAggregator.Subscribe<FolderUpdatedEvent>(this, e => folderUpdatedEvent = e);
        
        _folderDetailsViewModel.UpdateFolderNameCommand.Execute("Name");
        
        Assert.NotNull(folderUpdatedEvent);
        Assert.Equal(folder.Id, folderUpdatedEvent.UpdatedFolderId);
    }

    [Fact]
    public void DeleteFolderCommand_DeletesFolder()
    {
        var folder = new Folder("", 0);
        _eventAggregator.Publish(new FolderSelectedEvent(folder));
        
        _folderDetailsViewModel.DeleteFolderCommand.Execute(null);

        _foldersRepositoryMock.Received(1).Delete(Arg.Any<Folder[]>());
    }
    
    [Fact]
    public void DeleteFolderCommand_Publishes_FolderDeletedEvent()
    {
        var folder = new Folder("", 0);
        _eventAggregator.Publish(new FolderSelectedEvent(folder));
        
        FolderDeletedEvent? folderDeletedEvent = null;
        _eventAggregator.Subscribe<FolderDeletedEvent>(this, e => folderDeletedEvent = e);
        
        _folderDetailsViewModel.DeleteFolderCommand.Execute(null);
        
        Assert.NotNull(folderDeletedEvent);
        Assert.Equal(folder, folderDeletedEvent.DeletedFolder);
    }
}
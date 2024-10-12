using NSubstitute;
using Scribe.Data.Model;
using Scribe.Data.Repositories;
using Scribe.UI.Events;
using Scribe.UI.Views.Errors;
using Scribe.UI.Views.Sections.FolderDetails;

namespace Scribe.Tests.UI.Views;

public class FolderDetailsViewModelTests
{
    private readonly FolderDetailsViewModel _folderDetailsViewModel;
    private readonly EventAggregator _eventAggregator = new();
    private readonly IRepository<Document> _documentsRepositoryMock = Substitute.For<IRepository<Document>>();
    private readonly IRepository<Folder> _foldersRepositoryMock = Substitute.For<IRepository<Folder>>();

    public FolderDetailsViewModelTests()
    {
        _folderDetailsViewModel = new FolderDetailsViewModel(
            _eventAggregator, _foldersRepositoryMock, _documentsRepositoryMock
        );
        
        var testDocument = new Document(0, DateTime.Now, DateTime.Now);
        _documentsRepositoryMock.ImportFromFile(Arg.Any<string>()).Returns(testDocument);
        _documentsRepositoryMock.Add(Arg.Any<Document[]>()).Returns(info => ((Document[])info[0]).ToList());
    }

    [Fact]
    public void FolderSelectedEvent_Updates_SelectedFolder()
    {
        var folder = new Folder("", 0);
        
        _eventAggregator.Publish(new FolderSelectedEvent(folder));
        
        Assert.Equal(folder, _folderDetailsViewModel.SelectedFolder);
    }
    
    [Fact]
    public void SelctedFolderSetter_Exits_EditMode()
    {
        _folderDetailsViewModel.InEditMode = true;
        
        var folder = new Folder("", 0);
        _eventAggregator.Publish(new FolderSelectedEvent(folder));
        
        Assert.False(_folderDetailsViewModel.InEditMode);
    }
    
    [Fact]
    public void FolderNavigationPosition_Reflects_SelectedFolder()
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

        _foldersRepositoryMock.Received(1).Update(folder);
        Assert.Equal(newFolderName, _folderDetailsViewModel.SelectedFolder?.Name);
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
    
    [Fact]
    public void ExportFolder_Calls_ExportToFile()
    {
        var folder = new Folder("", 0);
        _eventAggregator.Publish(new FolderSelectedEvent(folder));

        const string directoryPath = "Path";
        
        _folderDetailsViewModel.ExportFolder(directoryPath);
        
        _foldersRepositoryMock.Received(1).ExportToFile(directoryPath, folder);
    }
    
    [Fact]
    public void ExportFolder_Raises_ExportError_For_DirectoryAndAcessException()
    {
        var folder = new Folder("", 0);
        _eventAggregator.Publish(new FolderSelectedEvent(folder));

        List<Exception> exceptions = [new DirectoryNotFoundException(), new UnauthorizedAccessException()];

        IViewModelError? viewModelError = null;
        _folderDetailsViewModel.ViewModelError += (_, error) => viewModelError = error; 

        foreach (var exception in exceptions)
        {
            _foldersRepositoryMock
                .ExportToFile(Arg.Any<string>(), Arg.Any<Folder>())
                .Returns(_ => throw exception);
            
            viewModelError = null;
        
            _folderDetailsViewModel.ExportFolder("");

            Assert.IsType<DocumentExportError>(viewModelError);
        }
    }

    [Fact]
    public void ImportDocumentsCommand_Calls_ImportFromFile()
    {
        var folder = new Folder("", 0);
        _eventAggregator.Publish(new FolderSelectedEvent(folder));

        var filePaths = new[] { "Path1", "Path2", "Path3" };

        _folderDetailsViewModel.ImportDocumentsCommand.Execute(filePaths);

        foreach (var path in filePaths)
        {
            _documentsRepositoryMock.Received(1).ImportFromFile(path);
        }
    }
        
    [Fact]
    public void ImportDocuments_Raises_ImportPathError_For_FileDirectoryAndAcessException()
    {
        var folder = new Folder("", 0);
        _eventAggregator.Publish(new FolderSelectedEvent(folder));
        
        IViewModelError? viewModelError = null;
        _folderDetailsViewModel.ViewModelError += (_, error) => viewModelError = error;

        var filePaths = new[] { "path" };
        
        List<Exception> exceptions = [
            new FileNotFoundException(), new DirectoryNotFoundException(), new UnauthorizedAccessException()
        ];
        
        foreach (var exception in exceptions)
        {
            _documentsRepositoryMock
                .ImportFromFile(Arg.Any<string>())
                .Returns<Task<Document>>(_ => throw exception);

            viewModelError = null;
        
            _folderDetailsViewModel.ImportDocumentsCommand.Execute(filePaths);
            
            Assert.IsType<DocumentImportFilePathError>(viewModelError);
            Assert.Equal(filePaths[0], ((DocumentImportFilePathError) viewModelError).FilePath);
        }
    }
    
    [Fact]
    public void ImportDocuments_Raises_ImportFileFormatError_For_FormatException()
    {
        var folder = new Folder("", 0);
        _eventAggregator.Publish(new FolderSelectedEvent(folder));
        
        _documentsRepositoryMock
            .ImportFromFile(Arg.Any<string>())
            .Returns<Task<Document>>(_ => throw new FormatException());
        
        IViewModelError? viewModelError = null;
        _folderDetailsViewModel.ViewModelError += (_, error) => viewModelError = error;
        
        var filePaths = new[] { "path" };
        _folderDetailsViewModel.ImportDocumentsCommand.Execute(filePaths);

        Assert.IsType<DocumentImportFileFormatError>(viewModelError);
        Assert.Equal(filePaths[0], ((DocumentImportFileFormatError) viewModelError).FilePath);
    }

    [Fact]
    public void ImportDocuments_Publishes_TagAddedEvent_ForEach_ImportedTag()
    {
        var folder = new Folder("", 0);
        _eventAggregator.Publish(new FolderSelectedEvent(folder));

        var importedDocument = new Document(0, DateTime.Now, DateTime.Now);
        var importedTags = new List<Tag> { new("Tag1", 0), new("Tag2", 0), new("Tag3", 0) }; 
        importedDocument.Tags = importedTags;

        _documentsRepositoryMock.ImportFromFile(Arg.Any<string>()).Returns(importedDocument);
        
        var createdTags = new List<Tag>();
        _eventAggregator.Subscribe<TagAddedEvent>(this, e => createdTags.Add(e.CreatedTag));
        
        _folderDetailsViewModel.ImportDocumentsCommand.Execute(new[] { "" });

        Assert.Equivalent(importedTags, createdTags);
    }
    
    [Fact]
    public void ImportDocuments_Adds_DocumentsToRepository()
    {
        var folder = new Folder("", 0);
        _eventAggregator.Publish(new FolderSelectedEvent(folder));
        
        var filePaths = new[] { "Path1", "Path2", "Path3" };
        _folderDetailsViewModel.ImportDocumentsCommand.Execute(filePaths);

        _documentsRepositoryMock.Received(1).Add(Arg.Any<Document[]>());
    }
    
    [Fact]
    public void ImportDocuments_Publishes_DocumentCreatedEvent_ForEach_ImportedDocument()
    {
        var folder = new Folder("", 0);
        _eventAggregator.Publish(new FolderSelectedEvent(folder));

        var importedDocuments = new List<Document>
        {
            new(0, DateTime.Now, DateTime.Now, "Doc1"),
            new(0, DateTime.Now, DateTime.Now, "Doc2"),
            new(0, DateTime.Now, DateTime.Now, "Doc3")
        };

        var filePaths = new[] { "Path1", "Path2", "Path3" };
        
        _documentsRepositoryMock.ImportFromFile(filePaths[0]).Returns(importedDocuments[0]);
        _documentsRepositoryMock.ImportFromFile(filePaths[1]).Returns(importedDocuments[1]);
        _documentsRepositoryMock.ImportFromFile(filePaths[2]).Returns(importedDocuments[2]);

        var createdDocuments = new List<Document>();
        _eventAggregator.Subscribe<DocumentCreatedEvent>(this, e => createdDocuments.Add(e.CreatedDocument));
        
        _folderDetailsViewModel.ImportDocumentsCommand.Execute(filePaths);

        Assert.Equivalent(importedDocuments, createdDocuments);
    }
}
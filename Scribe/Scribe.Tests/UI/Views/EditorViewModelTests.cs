using NSubstitute;
using Scribe.Data.Configurations;
using Scribe.Data.Model;
using Scribe.Data.Repositories;
using Scribe.UI.Events;
using Scribe.UI.Helpers;
using Scribe.UI.Views.Errors;
using Scribe.UI.Views.Sections.Editor;
using Scribe.UI.Views.Sections.Editor.State;

namespace Scribe.Tests.UI.Views;

public class EditorViewModelTests
{
    private readonly EditorViewModel _editorViewModel;
    private readonly EventAggregator _eventAggregator = new();
    private readonly IRepository<Document> _documentsRepositoryMock = Substitute.For<IRepository<Document>>();
    private readonly IConfigurationsRepository _configurationsRepositoryMock = Substitute.For<IConfigurationsRepository>();
    private readonly IPdfHelper _pdfHelperMock = Substitute.For<IPdfHelper>();

    public EditorViewModelTests() => _editorViewModel = new EditorViewModel(
        _eventAggregator, _documentsRepositoryMock, _configurationsRepositoryMock, _pdfHelperMock
    );

    [Fact]
    public void DocumentSelectedEvent_Adds_DocumentState()
    {
        var document = new Document(0, DateTime.Now, DateTime.Now);
        
        _eventAggregator.Publish(new DocumentSelectedEvent(document));

        Assert.Single(_editorViewModel.OpenDocuments);
        Assert.Equal(document, _editorViewModel.OpenDocuments[0].Document);
    }
    
    [Fact]
    public void DocumentSelectedEvent_IgnoresDocument_IfAlreadyOpen()
    {
        var document = new Document(0, DateTime.Now, DateTime.Now);
        
        _eventAggregator.Publish(new DocumentSelectedEvent(document));
        _eventAggregator.Publish(new DocumentSelectedEvent(document));

        Assert.Single(_editorViewModel.OpenDocuments);
    }
    
    [Fact]
    public void DocumentSelectedEvent_SelectsDocument()
    {
        var document = new Document(0, DateTime.Now, DateTime.Now);
        
        _eventAggregator.Publish(new DocumentSelectedEvent(document));
        
        Assert.Equal(document, _editorViewModel.SelectedDocument?.Document);
    }
    
    [Fact]
    public void FolderDeletedEvent_Closes_RelatedFolders()
    {
        var folder = new Folder("", 0);
        var documentA = new Document(folderId: 0, DateTime.Now, DateTime.Now);
        var documentB = new Document(folderId: 0, DateTime.Now, DateTime.Now);
        var documentC = new Document(folderId: 1, DateTime.Now, DateTime.Now);
        
        _eventAggregator.Publish(new DocumentSelectedEvent(documentA));
        _eventAggregator.Publish(new DocumentSelectedEvent(documentB));
        _eventAggregator.Publish(new DocumentSelectedEvent(documentC));

        _eventAggregator.Publish(new FolderDeletedEvent(folder));
        
        Assert.Single(_editorViewModel.OpenDocuments);
        Assert.Equal(documentC, _editorViewModel.OpenDocuments[0].Document);
    }
    
    [Fact]
    public void SelectedDocumentSetter_Updates_DocumentTags()
    {
        var document = new Document(0, DateTime.Now, DateTime.Now);
        var tags = new List<Tag> { new("TagA", 0), new("TagB", 0) };
        document.Tags = tags;

        _editorViewModel.SelectedDocument = new DocumentViewState(document);
        
        Assert.Equal(tags, _editorViewModel.DocumentTags);
    }
    
    [Fact]
    public void SelectedDocumentSetter_Sets_PreviewMode()
    {
        var document = new Document(0, DateTime.Now, DateTime.Now);
        
        _editorViewModel.SelectedDocument = new DocumentViewState(document);
        
        Assert.True(_editorViewModel.InPreviewMode);
        Assert.False(_editorViewModel.InEditMode);
    }
    
    [Fact]
    public void SelectedDocumentSetter_RaisesPropertyChanged_ForDocTimestamp()
    {
        var document = new Document(0, DateTime.Now, DateTime.Now);

        var eventRaised = false;
        _editorViewModel.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(_editorViewModel.DocumentTimestamp))
            {
                eventRaised = true;
            }
        };
        
        _editorViewModel.SelectedDocument = new DocumentViewState(document);

        Assert.True(eventRaised);
    }
    
    [Theory]
    [InlineData(LanguageConfiguration.PtBr)]
    [InlineData(LanguageConfiguration.EnUs)]
    public void CurrentLanguage_Reflects_RepositoryLanguage(LanguageConfiguration langConfig)
    {
        _configurationsRepositoryMock.GetAllConfigurations().Returns(
            new AppConfigurations(ThemeConfiguration.Light, langConfig, 1.0)    
        );
        
        Assert.Equal(langConfig, _editorViewModel.CurrentLanguage);
    }
    
    [Fact]
    public void ExportDocumentAsJson_Calls_ExportToFile()
    {
        var document = new Document(0, DateTime.Now, DateTime.Now);
        _editorViewModel.SelectedDocument = new DocumentViewState(document);

        const string directoryPath = "Path";
        _editorViewModel.ExportDocumentAsJson(directoryPath);
        
        _documentsRepositoryMock.Received(1).ExportToFile(directoryPath, document);
    }
    
    [Fact]
    public void ExportDocumentAsJson_RaisesExportError_For_DirectoryAndAcessException()
    {
        var document = new Document(0, DateTime.Now, DateTime.Now);
        _editorViewModel.SelectedDocument = new DocumentViewState(document);

        List<Exception> exceptions = [new DirectoryNotFoundException(), new UnauthorizedAccessException()];

        IViewModelError? viewModelError = null;
        _editorViewModel.ViewModelError += (_, error) => viewModelError = error; 

        foreach (var exception in exceptions)
        {
            _documentsRepositoryMock
                .ExportToFile(Arg.Any<string>(), Arg.Any<Document>())
                .Returns(_ => throw exception);
            
            viewModelError = null;
        
            _editorViewModel.ExportDocumentAsJson("");

            Assert.IsType<DocumentExportError>(viewModelError);
        }
    }
    
    [Fact]
    public void ExportDocumentAsPdf_Calls_ImageToPdf()
    {
        var document = new Document(0, DateTime.Now, DateTime.Now);
        _editorViewModel.SelectedDocument = new DocumentViewState(document);

        const string directoryPath = "Path";
        var documentAsImageBytes = new byte[] {1, 2, 3};
        _editorViewModel.ExportDocumentAsPdf(directoryPath, documentAsImageBytes);
        
        _pdfHelperMock.Received(1).ExportImageAsPdf(directoryPath, document.Name, documentAsImageBytes);
    }
    
    [Fact]
    public void ExportDocumentAsPdf_RaisesExportError_For_DirectoryAndAcessException()
    {
        var document = new Document(0, DateTime.Now, DateTime.Now);
        _editorViewModel.SelectedDocument = new DocumentViewState(document);

        List<Exception> exceptions = [new DirectoryNotFoundException(), new UnauthorizedAccessException()];

        IViewModelError? viewModelError = null;
        _editorViewModel.ViewModelError += (_, error) => viewModelError = error; 

        foreach (var exception in exceptions)
        {
            _documentsRepositoryMock
                .ExportToFile(Arg.Any<string>(), Arg.Any<Document>())
                .Returns(_ => throw exception);

            _pdfHelperMock
                .When(pdfHelper => pdfHelper.ExportImageAsPdf(
                    Arg.Any<string>(), Arg.Any<string>(), Arg.Any<byte[]>())
                )
                .Do(_ => throw exception);
            
            viewModelError = null;
        
            _editorViewModel.ExportDocumentAsPdf("", []);

            Assert.IsType<DocumentExportError>(viewModelError);
        }
    }
    
    [Fact]
    public void CloseDocumentCommand_ClosesDocument()
    {
        var document = new Document(0, DateTime.Now, DateTime.Now);
        _eventAggregator.Publish(new DocumentSelectedEvent(document));

        var selectedDocState = _editorViewModel.SelectedDocument;
        _editorViewModel.CloseDocumentCommand.Execute(selectedDocState);
        
        Assert.Empty(_editorViewModel.OpenDocuments);
        Assert.Null(_editorViewModel.SelectedDocument);
    }
    
    [Fact]
    public void CloseDocumentCommand_Selects_NextAvailableDocument()
    {
        var documentA = new Document(0, DateTime.Now, DateTime.Now);
        var documentB = new Document(0, DateTime.Now, DateTime.Now);
        _eventAggregator.Publish(new DocumentSelectedEvent(documentB));
        _eventAggregator.Publish(new DocumentSelectedEvent(documentA));

        var selectedDocState = _editorViewModel.SelectedDocument;
        _editorViewModel.CloseDocumentCommand.Execute(selectedDocState);
        
        Assert.Single(_editorViewModel.OpenDocuments);
        Assert.Equal(documentB, _editorViewModel.SelectedDocument?.Document);
    }
    
    [Fact]
    public void OpenDocumentByNameCommand_Publishes_SelectDocumentEvent()
    {
        var eventRaised = false;
        
        _eventAggregator.Subscribe<SelectDocumentByNameEvent>(this, _ => eventRaised = true);
        
        _editorViewModel.OpenDocumentByNameCommand.Execute("");
        
        Assert.True(eventRaised);
    }
    
    [Fact]
    public void SaveSelectedDocumentCommand_Updates_DocumentContent()
    {
        var document = new Document(0, DateTime.Now, DateTime.Now);
        _eventAggregator.Publish(new DocumentSelectedEvent(document));

        var selectedDocState = _editorViewModel.SelectedDocument!;
        const string editedContent = "EditedContent";
        selectedDocState.Document.Content = "";
        selectedDocState.EditedContent = editedContent;
        
        _editorViewModel.SaveSelectedDocumentCommand.Execute(null);

        Assert.Equal(editedContent, selectedDocState.Document.Content);
    }
    
    [Fact]
    public void SaveSelectedDocumentCommand_Updates_DocumentInRepository()
    {
        var document = new Document(0, DateTime.Now, DateTime.Now);
        _eventAggregator.Publish(new DocumentSelectedEvent(document));

        var selectedDocState = _editorViewModel.SelectedDocument!;
        selectedDocState.Document.Content = "EditedContent";
        
        _editorViewModel.SaveSelectedDocumentCommand.Execute(null);

        _documentsRepositoryMock.Received(1).Update([document]);
    }
    
    [Fact]
    public void SaveSelectedDocumentCommand_Resets_UnsavedChanges()
    {
        var document = new Document(0, DateTime.Now, DateTime.Now);
        _eventAggregator.Publish(new DocumentSelectedEvent(document));

        var selectedDocState = _editorViewModel.SelectedDocument!;
        selectedDocState.Document.Content = "EditedContent";
        selectedDocState.HasUnsavedChanges = true;
        
        _editorViewModel.SaveSelectedDocumentCommand.Execute(null);

        Assert.False(selectedDocState.HasUnsavedChanges);
    }
    
    [Fact]
    public void SaveSelectedDocumentCommand_RaisesPropertyChanged_ForDocTimestamp()
    {
        var document = new Document(0, DateTime.Now, DateTime.Now);
        _editorViewModel.SelectedDocument = new DocumentViewState(document);

        var eventRaised = false;
        _editorViewModel.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(_editorViewModel.DocumentTimestamp))
            {
                eventRaised = true;
            }
        };
        
        var selectedDocState = _editorViewModel.SelectedDocument!;
        selectedDocState.Document.Content = "EditedContent";
        
        _editorViewModel.SaveSelectedDocumentCommand.Execute(null);

        Assert.True(eventRaised);
    }
    
    [Fact]
    public void DeleteSelectedDocument_Deletes_DocumentInRepository()
    {
        var document = new Document(0, DateTime.Now, DateTime.Now);
        _eventAggregator.Publish(new DocumentSelectedEvent(document));
        
        _editorViewModel.DeleteSelectedDocumentCommand.Execute(null);

        _documentsRepositoryMock.Received(1).Delete([document]);
    }
    
    [Fact]
    public void DeleteSelectedDocument_Publishes_DocumentDeletedEvent()
    {
        var document = new Document(0, DateTime.Now, DateTime.Now);
        _eventAggregator.Publish(new DocumentSelectedEvent(document));

        var eventRaised = false;
        _eventAggregator.Subscribe<DocumentDeletedEvent>(this, _ => eventRaised = true);
        
        _editorViewModel.DeleteSelectedDocumentCommand.Execute(null);

        Assert.True(eventRaised);
    }
    
    [Fact]
    public void DeleteSelectedDocument_Publishes_TagRemovedEvent_ForEachDocTag()
    {
        var document = new Document(0, DateTime.Now, DateTime.Now);
        var tags = new List<Tag> { new("Tag1", 0), new("Tag2", 0), new("Tag3", 0) };
        document.Tags = tags;
        
        _eventAggregator.Publish(new DocumentSelectedEvent(document));

        var removedTags = new List<Tag>();
        _eventAggregator.Subscribe<TagRemovedEvent>(this, e => removedTags.Add(e.RemovedTag));
        
        _editorViewModel.DeleteSelectedDocumentCommand.Execute(null);
        
        Assert.Equivalent(tags, removedTags);
    }
    
    [Fact]
    public void DeleteSelectedDocument_ClosesDocument()
    {
        var document = new Document(0, DateTime.Now, DateTime.Now);
        _eventAggregator.Publish(new DocumentSelectedEvent(document));

        _editorViewModel.DeleteSelectedDocumentCommand.Execute(null);
        
        Assert.Empty(_editorViewModel.OpenDocuments);
        Assert.Null(_editorViewModel.SelectedDocument);
    }
    
    [Fact]
    public void UpdateSelectedDocumentNameCommand_Updates_DocumentName()
    {
        var document = new Document(0, DateTime.Now, DateTime.Now);
        _eventAggregator.Publish(new DocumentSelectedEvent(document));

        const string newDocumentName = "New Name";
        _editorViewModel.UpdateSelectedDocumentNameCommand.Execute(newDocumentName);
        
        Assert.Equal(newDocumentName, document.Name);
    }
    
    [Fact]
    public void UpdateSelectedDocumentNameCommand_Updates_DocumentInRepository()
    {
        var document = new Document(0, DateTime.Now, DateTime.Now);
        _eventAggregator.Publish(new DocumentSelectedEvent(document));

        const string newDocumentName = "New Name";
        _editorViewModel.UpdateSelectedDocumentNameCommand.Execute(newDocumentName);

        _documentsRepositoryMock.Received(1).Update([document]);
    }
    
    [Fact]
    public void UpdateSelectedDocumentNameCommand_Publishes_DocumentUpdatedEvent()
    {
        var document = new Document(0, DateTime.Now, DateTime.Now);
        _eventAggregator.Publish(new DocumentSelectedEvent(document));

        var eventRaised = false;
        _eventAggregator.Subscribe<DocumentUpdatedEvent>(this, _ => eventRaised = true);
        
        const string newDocumentName = "New Name";
        _editorViewModel.UpdateSelectedDocumentNameCommand.Execute(newDocumentName);

        Assert.True(eventRaised);
    }
    
    [Fact]
    public void UpdateSelectedDocumentNameCommand_RaisesPropertyChanged_ForSelectedDoc()
    {
        var document = new Document(0, DateTime.Now, DateTime.Now);
        _eventAggregator.Publish(new DocumentSelectedEvent(document));

        var eventRaised = false;
        _editorViewModel.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(_editorViewModel.SelectedDocument))
            {
                eventRaised = true;
            }
        };
        
        const string newDocumentName = "New Name";
        _editorViewModel.UpdateSelectedDocumentNameCommand.Execute(newDocumentName);

        Assert.True(eventRaised);
    }
    
    [Fact]
    public void UpdateSelectedDocumentNameCommand_Exits_EditMode()
    {
        var document = new Document(0, DateTime.Now, DateTime.Now);
        _eventAggregator.Publish(new DocumentSelectedEvent(document));

        _editorViewModel.InEditMode = true;
        
        _editorViewModel.UpdateSelectedDocumentNameCommand.Execute("");
        
        Assert.False(_editorViewModel.InEditMode);
    }
    
    [Fact]
    public void ToggleDocPinnedStatusCommand_Toggles_PinnedStatus()
    {
        var document = new Document(0, DateTime.Now, DateTime.Now);
        _eventAggregator.Publish(new DocumentSelectedEvent(document));
        
        _editorViewModel.ToggleSelectedDocumentPinnedStatusCommand.Execute("");
        Assert.True(_editorViewModel.SelectedDocument?.Document.IsPinned);
        
        _editorViewModel.ToggleSelectedDocumentPinnedStatusCommand.Execute("");
        Assert.False(_editorViewModel.SelectedDocument?.Document.IsPinned);
    }
    
    [Fact]
    public void ToggleDocPinnedStatusCommand_Updates_DocumentInRepository()
    {
        var document = new Document(0, DateTime.Now, DateTime.Now);
        _eventAggregator.Publish(new DocumentSelectedEvent(document));
        
        _editorViewModel.ToggleSelectedDocumentPinnedStatusCommand.Execute("");

        _documentsRepositoryMock.Received(1).Update([document]);
    }
    
    [Fact]
    public void ToggleDocPinnedStatusCommand_Publishes_DocumentUpdatedEvent()
    {
        var document = new Document(0, DateTime.Now, DateTime.Now);
        _eventAggregator.Publish(new DocumentSelectedEvent(document));

        var eventRaised = false;
        _eventAggregator.Subscribe<DocumentUpdatedEvent>(this, _ => eventRaised = true);
        
        _editorViewModel.ToggleSelectedDocumentPinnedStatusCommand.Execute("");

        Assert.True(eventRaised);
    }
    
    [Fact]
    public void ToggleDocPinnedStatusCommand_RaisesPropertyChanged_ForSelectedDoc()
    {
        var document = new Document(0, DateTime.Now, DateTime.Now);
        _eventAggregator.Publish(new DocumentSelectedEvent(document));

        var eventRaised = false;
        _editorViewModel.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(_editorViewModel.SelectedDocument))
            {
                eventRaised = true;
            }
        };
        
        _editorViewModel.ToggleSelectedDocumentPinnedStatusCommand.Execute("");

        Assert.True(eventRaised);
    }
    
    [Fact]
    public void EnterEditModeCommand_Enter_EditMode()
    {
        _editorViewModel.EnterEditModeCommand.Execute(null);

        Assert.True(_editorViewModel.InEditMode);
    }
    
    [Fact]
    public void SetInPreviewModeCommand_Sets_PreviewMode()
    {
        _editorViewModel.SetInPreviewModeCommand.Execute(true);
        Assert.True(_editorViewModel.InPreviewMode);
        
        _editorViewModel.SetInPreviewModeCommand.Execute(false);
        Assert.False(_editorViewModel.InPreviewMode);
    }
    
    [Fact]
    public void AddTagCommand_Adds_TagToDocument()
    {
        var document = new Document(0, DateTime.Now, DateTime.Now);
        _eventAggregator.Publish(new DocumentSelectedEvent(document));

        const string tagName = "TagName";
        _editorViewModel.AddTagCommand.Execute(tagName);

        Assert.Contains(
            _editorViewModel.SelectedDocument!.Document.Tags, 
            tag => tag.Name == tagName && tag.FolderId == document.FolderId
        );
        Assert.Contains(
            _editorViewModel.DocumentTags!, 
            tag => tag.Name == tagName && tag.FolderId == document.FolderId
        );
    }
    
    [Fact]
    public void AddTagCommand_Publishes_TagAddedEvent()
    {
        var document = new Document(0, DateTime.Now, DateTime.Now);
        _eventAggregator.Publish(new DocumentSelectedEvent(document));

        var eventRaised = false;
        _eventAggregator.Subscribe<TagAddedEvent>(this, _ => eventRaised = true);
        
        const string tagName = "TagName";
        _editorViewModel.AddTagCommand.Execute(tagName);

        Assert.True(eventRaised);
    }
    
    [Fact]
    public void AddTagCommand_Updates_DocumentInRepository()
    {
        var document = new Document(0, DateTime.Now, DateTime.Now);
        _eventAggregator.Publish(new DocumentSelectedEvent(document));
        
        const string tagName = "TagName";
        _editorViewModel.AddTagCommand.Execute(tagName);

        _documentsRepositoryMock.Received(1).Update([document]);
    }
    
    [Fact]
    public void AddTagCommand_IgnoresTag_IfAlreadyCreated()
    {
        const string tagName = "TagName";

        var document = new Document(0, DateTime.Now, DateTime.Now);
        var tag = new Tag(tagName, document.FolderId);
        document.Tags.Add(tag);
        
        var eventRaised = false;
        _eventAggregator.Subscribe<TagAddedEvent>(this, _ => eventRaised = true);
        
        _eventAggregator.Publish(new DocumentSelectedEvent(document));
        
        _editorViewModel.AddTagCommand.Execute(tagName);

        Assert.Single(_editorViewModel.SelectedDocument!.Document.Tags);
        Assert.Single(_editorViewModel.DocumentTags!);
        Assert.False(eventRaised);
        _documentsRepositoryMock.Received(0).Update([document]);
    }
    
    [Fact]
    public void RemoveTagCommand_Removes_TagFromDocument()
    {
        var document = new Document(0, DateTime.Now, DateTime.Now);
        var tag = new Tag("", document.FolderId);
        document.Tags.Add(tag);
        
        _eventAggregator.Publish(new DocumentSelectedEvent(document));

        _editorViewModel.RemoveTagCommand.Execute(tag);

        Assert.Empty(_editorViewModel.SelectedDocument!.Document.Tags);
        Assert.Empty(_editorViewModel.DocumentTags!);
    }
    
    [Fact]
    public void RemoveTagCommand_Publishes_TagRemovedEvent()
    {
        var document = new Document(0, DateTime.Now, DateTime.Now);
        var tag = new Tag("", document.FolderId);
        document.Tags.Add(tag);
        
        _eventAggregator.Publish(new DocumentSelectedEvent(document));

        var eventRaised = false;
        _eventAggregator.Subscribe<TagRemovedEvent>(this, _ => eventRaised = true);
        
        _editorViewModel.RemoveTagCommand.Execute(tag);

        Assert.True(eventRaised);
    }
    
    [Fact]
    public void RemoveTagCommand_Updates_DocumentInRepository()
    {
        var document = new Document(0, DateTime.Now, DateTime.Now);
        var tag = new Tag("", document.FolderId);
        document.Tags.Add(tag);
        
        _eventAggregator.Publish(new DocumentSelectedEvent(document));
        
        _editorViewModel.RemoveTagCommand.Execute(tag);

        _documentsRepositoryMock.Received(1).Update([document]);
    }
}
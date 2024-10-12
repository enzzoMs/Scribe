using NSubstitute;
using Scribe.Data.Model;
using Scribe.Data.Repositories;
using Scribe.UI.Events;
using Scribe.UI.Views.Sections.Documents;

namespace Scribe.Tests.UI.Views;

public class DocumentsViewModelTests
{
    private readonly DocumentsViewModel _documentsViewModel;
    private readonly EventAggregator _eventAggregator = new();
    private readonly IRepository<Document> _documentsRepositoryMock = Substitute.For<IRepository<Document>>();

    public DocumentsViewModelTests()
    {
        _documentsViewModel = new DocumentsViewModel(_eventAggregator, _documentsRepositoryMock);

        _documentsRepositoryMock.Add(Arg.Any<Document[]>()).Returns(info => ((Document[]) info[0]).ToList());
    }

    [Fact]
    public void FolderSelectEvent_Sets_AssociatedFolderAndDocuments()
    {
        var folder = new Folder("", 0);
        folder.Documents.Add(new Document(0, DateTime.Now, DateTime.Now));
        
        _eventAggregator.Publish(new FolderSelectedEvent(folder));
        
        Assert.Equal(folder, _documentsViewModel.AssociatedFolder);
        Assert.Equal(folder.Documents, _documentsViewModel.CurrentDocuments);
    }
    
    [Fact]
    public void SearchDocumentsFilter_Setter_FiltersDocuments()
    {
        var folder = new Folder("", 0);
        folder.Documents.Add(new Document(0, DateTime.Now, DateTime.Now, "No"));
        folder.Documents.Add(new Document(0, DateTime.Now, DateTime.Now, "Yes"));
        
        _eventAggregator.Publish(new FolderSelectedEvent(folder));
        
        _documentsViewModel.DelayDocumentsSearch = false;
        _documentsViewModel.SearchDocumentsFilter = "Yes";
        
        Assert.Single(_documentsViewModel.CurrentDocuments);
        Assert.Equal("Yes", _documentsViewModel.CurrentDocuments[0].Name);
    }

    [Fact]
    public void DocumentsAreSearched_By_NameAndTag()
    {
        var tagA = new Tag("TagA", 0); 
        var tagB = new Tag("TagB", 0); 

        var folder = new Folder("", 0);
        folder.Tags.Add(tagA);
        folder.Tags.Add(tagB);
        
        folder.Documents.Add(new Document(0, DateTime.Now, DateTime.Now, "No"));
        folder.Documents.Add(new Document(0, DateTime.Now, DateTime.Now, "Yes"));
        folder.Documents.Add(new Document(0, DateTime.Now, DateTime.Now, "Yes with TagA")
        {
            Tags = [tagA]
        });
        folder.Documents.Add(new Document(0, DateTime.Now, DateTime.Now, "Yes with TagA and TagB")
        {
            Tags = [tagA, tagB]
        });
        
        _eventAggregator.Publish(new FolderSelectedEvent(folder));
        _eventAggregator.Publish(new TagSelectionChangedEvent(tagA.Name, true));

        _documentsViewModel.DelayDocumentsSearch = false;
        _documentsViewModel.SearchDocumentsFilter = "Yes";
        
        Assert.Equal(2, _documentsViewModel.CurrentDocuments.Count);
        Assert.Equal("Yes with TagA", _documentsViewModel.CurrentDocuments[0].Name);
        Assert.Equal("Yes with TagA and TagB", _documentsViewModel.CurrentDocuments[1].Name);
    }
    
    [Fact]
    public void CreateDocumentCommand_AddsNewDocument()
    {
        var folder = new Folder("", 0);
        _eventAggregator.Publish(new FolderSelectedEvent(folder));
        
        _documentsViewModel.CreateDocumentCommand.Execute(null);
        
        _documentsRepositoryMock.Received(1).Add(Arg.Any<Document>());
        Assert.Single(_documentsViewModel.CurrentDocuments);
    }
    
    [Fact]
    public void CreateDocumentCommand_Publishes_DocumentCreatedEvent()
    {
        var folder = new Folder("", 0);
        _eventAggregator.Publish(new FolderSelectedEvent(folder));

        var eventRaised = true;
        _eventAggregator.Subscribe<DocumentCreatedEvent>(this, _ => eventRaised = true);
        
        _documentsViewModel.CreateDocumentCommand.Execute(null);
        
        Assert.True(eventRaised);
    }
    
    [Fact]
    public void OpenDocumentCommand_Publishes_DocumentSelectedEvent()
    {
        var folder = new Folder("", 0);
        var document = new Document(0, DateTime.Now, DateTime.Now);
        folder.Documents.Add(document);
        
        _eventAggregator.Publish(new FolderSelectedEvent(folder));

        Document? selectedDocument = null;
        _eventAggregator.Subscribe<DocumentSelectedEvent>(this, e => selectedDocument = e.SelectedDocument);
        
        _documentsViewModel.OpenDocumentCommand.Execute(document);
        
        Assert.Equal(document, selectedDocument);
    }
    
    [Fact]
    public void DocumentDeletedEvent_RemovesDocument()
    {
        var folder = new Folder("", 0);
        var document = new Document(0, DateTime.Now, DateTime.Now);
        folder.Documents.Add(document);
        
        _eventAggregator.Publish(new FolderSelectedEvent(folder));
        
        _eventAggregator.Publish(new DocumentDeletedEvent(document));

        Assert.Empty(folder.Documents);
        Assert.Empty(_documentsViewModel.CurrentDocuments);
    }

    [Fact]
    public void DocumentUpdatedEvent_ShowsAllDocuments()
    {
        var folder = new Folder("", 0);
        var documentA = new Document(0, DateTime.Now, DateTime.Now, "No");
        var documentB = new Document(0, DateTime.Now, DateTime.Now, "Yes");
        folder.Documents.Add(documentA);
        folder.Documents.Add(documentB);
        
        _eventAggregator.Publish(new FolderSelectedEvent(folder));

        _documentsViewModel.SearchDocumentsFilter = "Yes";
        
        _eventAggregator.Publish(new DocumentUpdatedEvent());
        
        Assert.Equal(2, _documentsViewModel.CurrentDocuments.Count);
        Assert.Empty(_documentsViewModel.SearchDocumentsFilter);
    }

    [Fact]
    public void DocumentCreatedEvent_AddsDocument()
    {
        var folder = new Folder("", 0);
        
        _eventAggregator.Publish(new FolderSelectedEvent(folder));

        var newDocument = new Document(0, DateTime.Now, DateTime.Now);
        _eventAggregator.Publish( new DocumentCreatedEvent(newDocument));

        Assert.Single(_documentsViewModel.CurrentDocuments);
        Assert.Equal(newDocument, _documentsViewModel.CurrentDocuments[0]);
    }

    [Fact]
    public void DocumentCreatedEvent_PublishesTagSelectionEvent_ForAllSelectedTags()
    {
        var tagA = new Tag("TagA", 0); 
        var tagB = new Tag("TagB", 0); 
        var tagNames = new List<string> { tagA.Name, tagB.Name };

        var folder = new Folder("", 0);
        folder.Tags.Add(tagA);
        folder.Tags.Add(tagB);

        _eventAggregator.Publish(new FolderSelectedEvent(folder));
        _eventAggregator.Publish(new TagSelectionChangedEvent(tagA.Name, IsSelected: true));
        _eventAggregator.Publish(new TagSelectionChangedEvent(tagB.Name, IsSelected: true));

        var unselectedTags = new List<string>();
        _eventAggregator.Subscribe<TagSelectionChangedEvent>(this, e =>
        {
            if (!e.IsSelected)
            {
                unselectedTags.Add(e.TagName);
            }
        });
        
        _eventAggregator.Publish(new DocumentCreatedEvent(new Document(0, DateTime.Now, DateTime.Now)));
        
        Assert.Equal(tagNames, unselectedTags);
    }
    
    [Fact]
    public void SelectDocumentByNameEvent_Publishes_DocumentSelectedEvent()
    {
        const string docName = "DocName";
        
        var folder = new Folder("", 0);
        var document = new Document(0, DateTime.Now, DateTime.Now, docName);
        folder.Documents.Add(document);

        Document? selectedDocument = null;
        _eventAggregator.Subscribe<DocumentSelectedEvent>(this, e => selectedDocument = e.SelectedDocument);
        
        _eventAggregator.Publish(new FolderSelectedEvent(folder));
        _eventAggregator.Publish(new SelectDocumentByNameEvent(docName));

        Assert.Equal(document, selectedDocument);
    }
    
    [Fact]
    public void SelectDocumentByNameEvent_IgnoresEvent_IfDocDoesNotExist()
    {
        const string docName = "DocName";
        
        var folder = new Folder("", 0);

        var eventRaised = false;
        _eventAggregator.Subscribe<DocumentSelectedEvent>(this, _ => eventRaised = true);
        
        _eventAggregator.Publish(new FolderSelectedEvent(folder));
        _eventAggregator.Publish(new SelectDocumentByNameEvent(docName));

        Assert.False(eventRaised);
    }
    
    [Fact]
    public void TagSelectionEvent_Filters_Documents()
    {
        var tagA = new Tag("TagA", 0); 
        var tagB = new Tag("TagB", 0); 
        var tagC = new Tag("TagC", 0); 

        var folder = new Folder("", 0);
        folder.Tags.Add(tagA);
        folder.Tags.Add(tagB);
        folder.Tags.Add(tagC);

        folder.Documents.Add(new Document(0, DateTime.Now, DateTime.Now, "Tags - A")
        {
            Tags = [tagA]
        });
        folder.Documents.Add(new Document(0, DateTime.Now, DateTime.Now, "Tags - A,B")
        {
            Tags = [tagA, tagB]
        });
        folder.Documents.Add(new Document(0, DateTime.Now, DateTime.Now, "Tags - C")
        {
            Tags = [tagC]
        });

        _eventAggregator.Publish(new FolderSelectedEvent(folder));
        _eventAggregator.Publish(new TagSelectionChangedEvent(tagA.Name, IsSelected: true));
        _eventAggregator.Publish(new TagSelectionChangedEvent(tagB.Name, IsSelected: true));
        _eventAggregator.Publish(new TagSelectionChangedEvent(tagC.Name, IsSelected: true));
        _eventAggregator.Publish(new TagSelectionChangedEvent(tagC.Name, IsSelected: false));

        Assert.Single(_documentsViewModel.CurrentDocuments);
        Assert.Equal("Tags - A,B", _documentsViewModel.CurrentDocuments[0].Name);
    }
    
    [Fact]
    public void TagAddedOrRemovedEvent_Filters_Documents()
    {
        var folder = new Folder("", 0);
        var tagA = new Tag("TagA", 0); 
        folder.Tags.Add(tagA);
        
        var documentTagA1 = new Document(0, DateTime.Now, DateTime.Now, "Tags - A1")
        {
            Tags = [tagA]
        };
        
        var documentTagA2 = new Document(0, DateTime.Now, DateTime.Now, "Tags - A2")
        {
            Tags = [tagA]
        };

        var documentNoTags = new Document(0, DateTime.Now, DateTime.Now, "Tags - None");
        
        folder.Documents.Add(documentTagA1);
        folder.Documents.Add(documentTagA2);
        folder.Documents.Add(documentNoTags);

        _eventAggregator.Publish(new FolderSelectedEvent(folder));
        _eventAggregator.Publish(new TagSelectionChangedEvent(tagA.Name, IsSelected: true));

        documentTagA2.Tags.Remove(tagA);
        _eventAggregator.Publish(new TagRemovedEvent(tagA));
        
        documentNoTags.Tags.Add(tagA);
        _eventAggregator.Publish(new TagAddedEvent(tagA));

        Assert.Equal(2, _documentsViewModel.CurrentDocuments.Count);
        Assert.Equal("Tags - A1", _documentsViewModel.CurrentDocuments[0].Name);
        Assert.Equal("Tags - None", _documentsViewModel.CurrentDocuments[1].Name);
    }
}
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
    
    public DocumentsViewModelTests() => 
        _documentsViewModel = new DocumentsViewModel(_eventAggregator, _documentsRepositoryMock);

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

        var newDocument = new Document(0, DateTime.Now, DateTime.Now, "NewDocument");
        
        _eventAggregator.Publish(new FolderSelectedEvent(folder));

        _documentsRepositoryMock.Add(Arg.Any<Document>()).Returns(newDocument);
        
        _documentsViewModel.CreateDocumentCommand.Execute(null);
        
        _documentsRepositoryMock.Received(1).Add(Arg.Any<Document>());
        Assert.Single(_documentsViewModel.CurrentDocuments);
        Assert.Equal(newDocument, _documentsViewModel.CurrentDocuments[0]);
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
    public void DocumentUpdatedEvent_Raises_PropertyChangedEvent()
    {
        var folder = new Folder("", 0);
        var document = new Document(0, DateTime.Now, DateTime.Now);
        folder.Documents.Add(document);
        
        _eventAggregator.Publish(new FolderSelectedEvent(folder));

        var eventRaised = false;
        _documentsViewModel.PropertyChanged += (_, _) => eventRaised = true;
        
        _eventAggregator.Publish(new DocumentUpdatedEvent());
        
        Assert.True(eventRaised);
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
}
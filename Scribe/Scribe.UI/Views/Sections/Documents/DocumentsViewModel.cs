using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Scribe.Data.Model;
using Scribe.Data.Repositories;
using Scribe.UI.Command;
using Scribe.UI.Events;

namespace Scribe.UI.Views.Sections.Documents;

public class DocumentsViewModel : BaseViewModel
{
    private readonly IEventAggregator _eventAggregator;
    private readonly IRepository<Document> _documentsRepository;
    
    private List<Document> _allDocuments = [];
    private ObservableCollection<Document> _currentDocuments = [];

    private readonly SortedSet<string> _selectedTagNames = [];
    
    private Folder? _associatedFolder;
    
    private string _searchDocumentsFilter = "";
    private readonly DispatcherTimer _searchTimer;
    private const int SearchDelayMs = 800;

    public DocumentsViewModel(IEventAggregator eventAggregator, IRepository<Document> documentsRepository)
    {
        _eventAggregator = eventAggregator;
        eventAggregator.Subscribe<FolderSelectedEvent>(this, OnFolderSelected);
        eventAggregator.Subscribe<DocumentDeletedEvent>(this, OnDocumentDeleted);
        eventAggregator.Subscribe<DocumentUpdatedEvent>(this, OnDocumentUpdated);
        eventAggregator.Subscribe<TagAddedEvent>(this, e => OnTagAddedOrRemoved(e.CreatedTag.Name));
        eventAggregator.Subscribe<TagRemovedEvent>(this, e => OnTagAddedOrRemoved(e.RemovedTag.Name));
        eventAggregator.Subscribe<TagSelectionChangedEvent>(this, e =>
        {
            OnTagSelectionChanged(e);
            FilterDocuments();
        });

        _documentsRepository = documentsRepository;
        
        _searchTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(SearchDelayMs) };
        _searchTimer.Tick += (_, _) =>
        {
            _searchTimer.Stop();
            FilterDocuments();
        };
        
        CreateDocumentCommand = new DelegateCommand(_ => CreateDocument());
        OpenDocumentCommand = new DelegateCommand(param =>
        {
            if (param is Document doc) 
                eventAggregator.Publish(new DocumentSelectedEvent(doc));
        });
    }
    
    public ObservableCollection<Document> CurrentDocuments
    {
        get => _currentDocuments;
        set
        {
            _currentDocuments = value;
            RaisePropertyChanged();
        }
    }

    public Folder? AssociatedFolder
    {
        get => _associatedFolder;
        set
        {
            _associatedFolder = value;
            RaisePropertyChanged();
        }
    }
    
    public string SearchDocumentsFilter
    {
        get => _searchDocumentsFilter;
        set
        {
            _searchDocumentsFilter = value;

            if (DelayDocumentsSearch)
            {
                _searchTimer.Stop();
                _searchTimer.Start();
            }
            else
            {
                FilterDocuments();
            }
        }
    }

    public bool DelayDocumentsSearch { get; set; }
    
    public ICommand CreateDocumentCommand { get; }

    public ICommand OpenDocumentCommand { get; }
    
    private void FilterDocuments()
    {
        var filterText = _searchDocumentsFilter.Trim();
        var filteredDocuments = _allDocuments
            .Where(doc => _selectedTagNames.IsSubsetOf(doc.Tags.Select(tag => tag.Name)))
            .Where(doc => doc.Name.Contains(filterText, StringComparison.CurrentCultureIgnoreCase))
            .OrderByDescending(d => d.IsPinned);
        
        CurrentDocuments = new ObservableCollection<Document>(filteredDocuments);
    }

    private void ShowAllDocuments()
    {
        _searchDocumentsFilter = "";
        RaisePropertyChanged(nameof(SearchDocumentsFilter));
        CurrentDocuments = new ObservableCollection<Document>(
            _allDocuments
                .Where(doc => _selectedTagNames.IsSubsetOf(doc.Tags.Select(tag => tag.Name)))
                .OrderByDescending(d => d.IsPinned)
        );
    }

    private async void CreateDocument()
    {
        if (_associatedFolder == null) return;

        var documentName = Application.Current?.TryFindResource("String.Documents.DefaultName") as string ?? "New Document";

        var createdTimestamp = DateTime.Now;

        var newDocument = await _documentsRepository.Add(new Document(
            folderId: _associatedFolder.Id,
            createdTimestamp: createdTimestamp,
            lastModifiedTimestamp: createdTimestamp,
            name: documentName
        ));

        foreach (var tagName in _selectedTagNames.ToList())
        {
            _eventAggregator.Publish(new TagSelectionChangedEvent(tagName, IsSelected: false));
        }
        
        ShowAllDocuments();

        _associatedFolder.Documents.Add(newDocument);
        _allDocuments.Add(newDocument);

        if (_selectedTagNames.Count == 0)
        {
            CurrentDocuments.Add(newDocument);
        }
    }

    private void OnFolderSelected(FolderSelectedEvent folderEvent)
    {
        AssociatedFolder = folderEvent.Folder;

        if (_associatedFolder == null) return;
            
        _selectedTagNames.Clear();
        _allDocuments = _associatedFolder.Documents.ToList();
        
        ShowAllDocuments();
    }

    private void OnDocumentDeleted(DocumentDeletedEvent documentEvent)
    {
        ShowAllDocuments();

        _associatedFolder?.Documents.Remove(documentEvent.DeletedDocument);
        _allDocuments.Remove(documentEvent.DeletedDocument);
        CurrentDocuments.Remove(documentEvent.DeletedDocument);
    }

    private void OnDocumentUpdated(DocumentUpdatedEvent documentEvent) => ShowAllDocuments();

    private void OnTagAddedOrRemoved(string tagName)
    {
        if (_selectedTagNames.Contains(tagName))
        {
            FilterDocuments();
        }
    }
    
    private void OnTagSelectionChanged(TagSelectionChangedEvent tagEvent)
    {
        if (tagEvent.IsSelected)
        {
            _selectedTagNames.Add(tagEvent.TagName);
        }
        else
        {
            _selectedTagNames.Remove(tagEvent.TagName);
        }
    }
}
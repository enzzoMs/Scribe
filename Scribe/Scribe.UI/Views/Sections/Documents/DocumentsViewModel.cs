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

    private Folder? _associatedFolder;
    
    private string _searchDocumentsFilter = "";
    private readonly DispatcherTimer _searchTimer;
    private const int SearchDelayMs = 800;

    public DocumentsViewModel(IEventAggregator eventAggregator, IRepository<Document> documentsRepository)
    {
        _eventAggregator = eventAggregator;
        _documentsRepository = documentsRepository;
        
        _eventAggregator.Subscribe<FolderSelectedEvent>(this, folderEvent =>
        {
            AssociatedFolder = folderEvent.Folder;

            if (_associatedFolder == null) return;
            
            _allDocuments = _associatedFolder.Documents.ToList();
            CurrentDocuments = new ObservableCollection<Document>(_allDocuments);
        });
        
        _eventAggregator.Subscribe<DocumentDeletedEvent>(this, OnDocumentDeleted);
        _eventAggregator.Subscribe<DocumentUpdatedEvent>(this, OnDocumentUpdated);
        
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
                _eventAggregator.Publish(new DocumentSelectedEvent(doc));
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
            _searchTimer.Stop();
            _searchTimer.Start();
        }
    }
    
    public ICommand CreateDocumentCommand { get; }

    public ICommand OpenDocumentCommand { get; }
    
    private void FilterDocuments()
    {
        var filterText = _searchDocumentsFilter.Trim();
        var filteredDocuments = _allDocuments.Where(folder => folder.Name.Contains(
            filterText, StringComparison.CurrentCultureIgnoreCase
        ));
        CurrentDocuments = new ObservableCollection<Document>(filteredDocuments);
    }

    private void ShowAllDocuments()
    {
        _searchDocumentsFilter = "";
        RaisePropertyChanged(nameof(SearchDocumentsFilter));
        CurrentDocuments = new ObservableCollection<Document>(_allDocuments);
    }

    private async void CreateDocument()
    {
        if (_associatedFolder == null) return;
        
        var documentName = (string) Application.Current.TryFindResource("String.Documents.DefaultName") ?? "New Folder";

        var createdTimestamp = DateTime.Now;

        var newDocumentDocument = await _documentsRepository.Add(new Document(
            folderId: _associatedFolder.Id,
            createdTimestamp: createdTimestamp,
            lastModifiedTimestamp: createdTimestamp,
            name: documentName
        ));
        
        _associatedFolder.Documents.Add(newDocumentDocument);
        _allDocuments.Add(newDocumentDocument);
        ShowAllDocuments();
    }

    private void OnDocumentDeleted(DocumentDeletedEvent documentEvent)
    {
        _associatedFolder?.Documents.Remove(documentEvent.DeletedDocument);
        _allDocuments.Remove(documentEvent.DeletedDocument);
        
        ShowAllDocuments();
    }

    private void OnDocumentUpdated(DocumentUpdatedEvent documentEvent) => ShowAllDocuments();
}
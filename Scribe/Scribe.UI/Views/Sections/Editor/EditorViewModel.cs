using System.Collections.ObjectModel;
using System.Windows.Input;
using Scribe.Data.Model;
using Scribe.Data.Repositories;
using Scribe.UI.Command;
using Scribe.UI.Events;

namespace Scribe.UI.Views.Sections.Editor;

public class EditorViewModel : BaseViewModel
{
    private readonly IEventAggregator _eventAggregator; 
    private readonly IRepository<Document> _documentsRepository;
    
    private Document? _selectedDocument;

    private bool _onEditMode;
    
    public EditorViewModel(IEventAggregator eventAggregator, IRepository<Document> documentsRepository)
    {
        _documentsRepository = documentsRepository;
        _eventAggregator = eventAggregator;
        
        _eventAggregator.Subscribe<DocumentSelectedEvent>(this, OnDocumentSelected);
        _eventAggregator.Subscribe<FolderDeletedEvent>(this, OnFolderDeleted);
        
        CloseDocumentCommand = new DelegateCommand(param =>
        {
            if (param is Document doc) CloseDocument(doc);
        });
        DeleteDocumentCommand = new DelegateCommand(_ => DeleteSelectedDocument());
        EnterEditModeCommand = new DelegateCommand(_ => OnEditMode = true);
        UpdateDocumentNameCommand = new DelegateCommand(param =>
        {
            if (param is not string newDocumentName) return;
            UpdateSelectedDocumentName(newDocumentName);
            OnEditMode = false;
        });
        ToggleDocumentPinnedStatusCommand = new DelegateCommand(_ => ToggleDocumentPinnedStatus());
    }
    
    public ObservableCollection<Document> OpenDocuments { get; } = [];

    public Document? SelectedDocument { 
        get => _selectedDocument;
        set
        {
            _selectedDocument = value;
            OnEditMode = false;
            RaisePropertyChanged();
        }
    }

    public bool OnEditMode
    {
        get => _onEditMode;
        set
        {
            _onEditMode = value;
            RaisePropertyChanged();
        }
    }

    public ICommand CloseDocumentCommand { get; }

    public ICommand DeleteDocumentCommand { get; }
    
    public ICommand EnterEditModeCommand { get; }

    public ICommand UpdateDocumentNameCommand { get; }

    public ICommand ToggleDocumentPinnedStatusCommand { get; }
    
    private void CloseDocument(Document document)
    {
        var documentIndex = OpenDocuments.IndexOf(document);

        OpenDocuments.Remove(document);

        if (_selectedDocument == document)
        {
            Document? nextSelectedDocument = null;

            if (documentIndex == 0 && OpenDocuments.Count >= 1) 
                nextSelectedDocument = OpenDocuments[0];
            else if (OpenDocuments.Count >= 1)
                nextSelectedDocument = OpenDocuments[documentIndex - 1];

            SelectedDocument = nextSelectedDocument;
        }
    }

    private async void DeleteSelectedDocument()
    {
        if (_selectedDocument == null) return;
        
        await _documentsRepository.Delete(_selectedDocument);

        _eventAggregator.Publish(new DocumentDeletedEvent(_selectedDocument));
        
        CloseDocument(_selectedDocument);
    }
    
    private async void UpdateSelectedDocumentName(string newDocumentName)
    {
        if (_selectedDocument == null || _selectedDocument.Name == newDocumentName) return;

        _selectedDocument.Name = newDocumentName.Trim();
        await _documentsRepository.Update(_selectedDocument);
        
        // Forcing an update on the 'SelectedDocument tab'
        var doc = _selectedDocument;
        SelectedDocument = null;
        SelectedDocument = doc;
        
        _eventAggregator.Publish(new DocumentUpdatedEvent());
    }

    private async void ToggleDocumentPinnedStatus()
    {
        if (_selectedDocument == null) return;

        _selectedDocument.IsPinned = !_selectedDocument.IsPinned;

        await _documentsRepository.Update(_selectedDocument);
        
        RaisePropertyChanged(nameof(SelectedDocument));
        _eventAggregator.Publish(new DocumentUpdatedEvent());
    }

    private void OnDocumentSelected(DocumentSelectedEvent docEvent)
    {
        if (!OpenDocuments.Contains(docEvent.Document))
            OpenDocuments.Add(docEvent.Document);

        SelectedDocument = docEvent.Document;
    }

    private void OnFolderDeleted(FolderDeletedEvent folderEvent)
    {
        var deletedFolderId = folderEvent.DeletedFolder.Id;
        var deletedDocuments = OpenDocuments.Where(d => d.FolderId == deletedFolderId).ToList();

        foreach (var document in deletedDocuments)
        {
            CloseDocument(document);
        }
    }
}
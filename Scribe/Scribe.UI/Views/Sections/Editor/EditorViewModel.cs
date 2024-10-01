using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Input;
using Scribe.Data.Model;
using Scribe.Data.Repositories;
using Scribe.UI.Command;
using Scribe.UI.Events;
using Scribe.UI.Helpers;
using Scribe.UI.Views.Errors;
using Scribe.UI.Views.Sections.Editor.State;

namespace Scribe.UI.Views.Sections.Editor;

public class EditorViewModel : BaseViewModel
{
    private readonly IEventAggregator _eventAggregator; 
    private readonly IRepository<Document> _documentsRepository;
    
    private DocumentViewState? _selectedDocument;
    private ObservableCollection<Tag>? _documentTags;
    
    private bool _inEditMode;
    private bool _inPreviewMode;
    
    public EditorViewModel(IEventAggregator eventAggregator, IRepository<Document> documentsRepository)
    {
        _documentsRepository = documentsRepository;
        _eventAggregator = eventAggregator;
        
        _eventAggregator.Subscribe<DocumentSelectedEvent>(this, OnDocumentSelected);
        _eventAggregator.Subscribe<FolderDeletedEvent>(this, OnFolderDeleted);
        
        CloseDocumentCommand = new DelegateCommand(param =>
        {
            if (param is not DocumentViewState docState) return;
            CloseDocument(docState);
        });
        OpenDocumentByNameCommand = new DelegateCommand(param =>
        {
            if (param is not string docName) return;
            _eventAggregator.Publish(new SelectDocumentByNameEvent(docName));
            
        });
        SaveAndCloseDocumentCommand = new DelegateCommand(param =>
        {
            if (param is not DocumentViewState docState) return;
            SaveDocumentContent(docState);
            CloseDocument(docState);
        });
        SaveSelectedDocumentCommand = new DelegateCommand(_ =>
        {
            if (_selectedDocument == null) return;
            SaveDocumentContent(_selectedDocument);
        });
        DeleteSelectedDocumentCommand = new DelegateCommand(_ => DeleteSelectedDocument());
        UpdateSelectedDocumentNameCommand = new DelegateCommand(param =>
        {
            if (param is not string newDocumentName) return;
            UpdateSelectedDocumentName(newDocumentName);
            InEditMode = false;
        });
        ToggleSelectedDocumentPinnedStatusCommand = new DelegateCommand(_ => ToggleSelectedDocumentPinnedStatus());
        EnterEditModeCommand = new DelegateCommand(_ => InEditMode = true);
        AddTagCommand = new DelegateCommand(param =>
        {
            if (param is not string tagName) return;
            AddTag(tagName.Trim());
        });
        RemoveTagCommand = new DelegateCommand(param =>
        {
            if (param is not Tag tag) return;
            RemoveTag(tag);
        });
        SetInPreviewModeCommand = new DelegateCommand(param =>
        {
            if (param is not bool onPreviewMode) return;
            InPreviewMode = onPreviewMode;
        });
    }
    
    public ObservableCollection<DocumentViewState> OpenDocuments { get; } = [];

    public DocumentViewState? SelectedDocument { 
        get => _selectedDocument;
        set
        {
            _selectedDocument = value;
            
            if (_selectedDocument != null)
            {
                DocumentTags = new ObservableCollection<Tag>(_selectedDocument.Document.Tags);   
            }

            InPreviewMode = true;
            InEditMode = false;
            
            RaisePropertyChanged();
            RaisePropertyChanged(nameof(DocumentTimestamp));
        }
    }

    public ObservableCollection<Tag>? DocumentTags
    {
        get => _documentTags;
        private set
        {
            _documentTags = value;
            RaisePropertyChanged();
        }
    }

    public string? DocumentTimestamp =>
        _selectedDocument == null ? null : 
            $"{_selectedDocument.Document.CreatedTimestamp.ToShortDateString()} - " +
            _selectedDocument.Document.LastModifiedTimestamp.ToShortDateString();

    public bool InEditMode
    {
        get => _inEditMode;
        set
        {
            _inEditMode = value;
            RaisePropertyChanged();
        }
    }
    
    public bool InPreviewMode
    {
        get => _inPreviewMode;
        set
        {
            _inPreviewMode = value;
            RaisePropertyChanged();
        }
    }
    
    public ICommand CloseDocumentCommand { get; }

    public ICommand OpenDocumentByNameCommand { get; }
    
    public ICommand SaveAndCloseDocumentCommand { get; }

    public ICommand SaveSelectedDocumentCommand { get; }
    
    public ICommand DeleteSelectedDocumentCommand { get; }
    
    public ICommand UpdateSelectedDocumentNameCommand { get; }
    
    public ICommand ToggleSelectedDocumentPinnedStatusCommand { get; }
    
    public ICommand EnterEditModeCommand { get; }

    public ICommand AddTagCommand { get; }

    public ICommand RemoveTagCommand { get; }
    
    public ICommand SetInPreviewModeCommand { get; }
    
    public async void ExportDocumentAsJson(string directoryPath)
    {
        if (_selectedDocument == null) return;
        try
        {
            await _documentsRepository.ExportToFile(directoryPath, _selectedDocument.Document);
        }
        catch (Exception e) when (e is DirectoryNotFoundException or UnauthorizedAccessException)
        {
            RaiseViewModelError(new DocumentExportError());
        }
    }

    public void ExportDocumentAsPdf(string directoryPath, byte[] markupAsImage)
    {
        if (_selectedDocument == null) return;
        try
        {
            PdfHelper.ExportImageAsPdf(directoryPath, _selectedDocument.Document.Name, markupAsImage);   
        }
        catch (Exception e) when (e is DirectoryNotFoundException or UnauthorizedAccessException)
        {
            RaiseViewModelError(new DocumentExportError());
        }
    }
    
    private void CloseDocument(DocumentViewState documentViewState)
    {
        var documentIndex = OpenDocuments.IndexOf(documentViewState);

        OpenDocuments.Remove(documentViewState);

        if (_selectedDocument == documentViewState)
        {
            DocumentViewState? nextSelectedDocument = null;

            if (documentIndex == 0 && OpenDocuments.Count >= 1) 
                nextSelectedDocument = OpenDocuments[0];
            else if (OpenDocuments.Count >= 1)
                nextSelectedDocument = OpenDocuments[documentIndex - 1];

            SelectedDocument = nextSelectedDocument;
        }
    }
    
    private async void SaveDocumentContent(DocumentViewState documentViewState)
    {
        if (documentViewState.Document.Content != documentViewState.EditedContent)
        {
            documentViewState.Document.Content = documentViewState.EditedContent;
            documentViewState.Document.LastModifiedTimestamp = DateTime.Now;
        
            await _documentsRepository.Update(documentViewState.Document);
        }

        documentViewState.HasUnsavedChanges = false;
        
        RaisePropertyChanged(nameof(DocumentTimestamp));
    }
    
    private async void DeleteSelectedDocument()
    {
        if (_selectedDocument == null) return;
        
        await _documentsRepository.Delete(_selectedDocument.Document);

        _eventAggregator.Publish(new DocumentDeletedEvent(_selectedDocument.Document));
        
        foreach (var tag in _selectedDocument.Document.Tags)
        {
            _eventAggregator.Publish(new TagRemovedEvent(tag));
        }
        
        CloseDocument(_selectedDocument);
    }
    
    private async void UpdateSelectedDocumentName(string newDocumentName)
    {
        if (_selectedDocument == null || _selectedDocument.Document.Name == newDocumentName) return;

        _selectedDocument.Document.Name = newDocumentName.Trim();
        await _documentsRepository.Update(_selectedDocument.Document);
        
        _eventAggregator.Publish(new DocumentUpdatedEvent());
        
        RaisePropertyChanged(nameof(SelectedDocument));
    }

    private async void ToggleSelectedDocumentPinnedStatus()
    {
        if (_selectedDocument == null) return;

        _selectedDocument.Document.IsPinned = !_selectedDocument.Document.IsPinned;

        await _documentsRepository.Update(_selectedDocument.Document);
        
        RaisePropertyChanged(nameof(SelectedDocument));
        _eventAggregator.Publish(new DocumentUpdatedEvent());
    }

    private async void AddTag(string tagName)
    {
        if (_selectedDocument == null || tagName == "" || _documentTags!.Any(tag => tag.Name == tagName)) return;

        var newTag = new Tag(tagName, _selectedDocument.Document.FolderId);
        
        _selectedDocument.Document.Tags.Add(newTag);
        _documentTags!.Add(newTag);
        
        _eventAggregator.Publish(new TagAddedEvent(newTag));
        
        await _documentsRepository.Update(_selectedDocument.Document);
    }

    private async void RemoveTag(Tag tag)
    {
        if (_selectedDocument == null) return;
        
        _selectedDocument.Document.Tags.Remove(tag);
        _documentTags!.Remove(tag);

        await _documentsRepository.Update(_selectedDocument.Document);

        _eventAggregator.Publish(new TagRemovedEvent(tag));
    }

    private void OnDocumentSelected(DocumentSelectedEvent docEvent)
    {
        var documentState = OpenDocuments.FirstOrDefault(docState => docState.Document == docEvent.Document);
        
        if (documentState == null)
        {
            documentState = new DocumentViewState(docEvent.Document);
            OpenDocuments.Add(documentState);
        }

        SelectedDocument = documentState;
    }

    private void OnFolderDeleted(FolderDeletedEvent folderEvent)
    {
        var deletedFolderId = folderEvent.DeletedFolder.Id;
        var deletedDocuments = OpenDocuments.Where(docState => docState.Document.FolderId == deletedFolderId).ToList();

        foreach (var document in deletedDocuments)
        {
            CloseDocument(document);
        }
    }
}
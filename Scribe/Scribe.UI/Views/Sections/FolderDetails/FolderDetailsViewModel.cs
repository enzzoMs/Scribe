using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Scribe.Data.Model;
using Scribe.Data.Repositories;
using Scribe.UI.Command;
using Scribe.UI.Events;

namespace Scribe.UI.Views.Sections.FolderDetails;

public class FolderDetailsViewModel : BaseViewModel
{
    private readonly IEventAggregator _eventAggregator;
    private readonly IRepository<Folder> _foldersRepository;
    private readonly IRepository<Document> _documentsRepository;

    private Folder? _currentFolder;

    private List<Document> _allDocuments = [];
    private ObservableCollection<Document> _currentDocuments = [];
    
    private readonly DispatcherTimer _searchTimer;
    private string _searchDocumentsFilter = "";
    private const int SearchDelayMs = 800;

    private bool _onEditMode;
    
    public FolderDetailsViewModel(
        IEventAggregator eventAggregator, IRepository<Folder> foldersRepository, IRepository<Document> documentsRepository)
    {
        _eventAggregator = eventAggregator;
        _foldersRepository = foldersRepository;
        _documentsRepository = documentsRepository;
        
        _eventAggregator.Subscribe<FolderSelectedEvent>(this, eventData => CurrentFolder = eventData.Folder);
        
        _searchTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(SearchDelayMs) };
        _searchTimer.Tick += (_, _) =>
        {
            _searchTimer.Stop();
            FilterDocuments();
        };

        SetOnEditModeCommand = new DelegateCommand(param =>
        {
            if (param is bool onEditMode) OnEditMode = onEditMode;
        });
        UpdateFolderNameCommand = new DelegateCommand(param =>
        {
            if (param is not string newFolderName) return;
            OnEditMode = false;
            UpdateFolderName(newFolderName);
        });
        DeleteFolderCommand = new DelegateCommand(_ => DeleteFolder());
        CreateDocumentCommand = new DelegateCommand(_ => CreateDocument());
        OpenDocumentCommand = new DelegateCommand(param =>
        {
            if (param is Document doc) 
                _eventAggregator.Publish(new DocumentSelectedEvent(doc));
        });
    }
    
    public Folder? CurrentFolder
    {
        get => _currentFolder;
        set
        {
            _currentFolder = value;
            OnEditMode = false;
            LoadDocuments();
            
            RaisePropertyChanged();
            RaisePropertyChanged(nameof(FolderNavigationPosition));
        }
    }
    
    public int FolderNavigationPosition
    {
        get => _currentFolder?.NavigationIndex + 1 ?? 1;
        set => UpdateFolderPosition(value - 1);
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

    public bool OnEditMode
    {
        get => _onEditMode;
        private set
        {
            _onEditMode = value;
            RaisePropertyChanged();
        }
    }

    public ICommand UpdateFolderNameCommand { get; }

    public ICommand DeleteFolderCommand { get; }
    
    public ICommand SetOnEditModeCommand { get; }
    
    public ICommand CreateDocumentCommand { get; }

    public ICommand OpenDocumentCommand { get; }

    private async void UpdateFolderName(string newFolderName)
    {
        if (_currentFolder == null || _currentFolder.Name == newFolderName) return;

        _currentFolder.Name = newFolderName;
        await _foldersRepository.Update(_currentFolder);
        
        RaisePropertyChanged(nameof(CurrentFolder));
        _eventAggregator.Publish(new FolderUpdatedEvent(_currentFolder.Id));
    }

    private void UpdateFolderPosition(int newIndex)
    {
        if (_currentFolder == null || _currentFolder.NavigationIndex == newIndex) return;

        var oldIndex = _currentFolder.NavigationIndex;
        
        _eventAggregator.Publish(new FolderPositionUpdatedEvent(_currentFolder.Id, oldIndex, newIndex));
    }

    private async void DeleteFolder()
    {
        if (_currentFolder == null) return;
        
        await _foldersRepository.Delete(_currentFolder);
        
        _eventAggregator.Publish(new FolderDeletedEvent(_currentFolder));
    }
    
    private void FilterDocuments()
    {
        var filterText = _searchDocumentsFilter.Trim();
        var filteredDocuments = _allDocuments.Where(folder => folder.Name.Contains(
            filterText, StringComparison.CurrentCultureIgnoreCase
        ));
        CurrentDocuments = new ObservableCollection<Document>(filteredDocuments);
    }

    private void ClearDocumentsFilter()
    {
        _searchDocumentsFilter = "";
        RaisePropertyChanged(nameof(SearchDocumentsFilter));
        FilterDocuments();
    }

    private void LoadDocuments()
    {
        if (_currentFolder == null) return;
        
        _allDocuments = _currentFolder.Documents.ToList();
        CurrentDocuments = new ObservableCollection<Document>(_allDocuments);
    }

    private async void CreateDocument()
    {
        if (_currentFolder == null) return;

        ClearDocumentsFilter();
        
        var documentName = (string) Application.Current.TryFindResource("String.Documents.DefaultName") ?? "New Folder";

        var createdTimestamp = DateTime.Now;

        var newDocumentDocument = await _documentsRepository.Add(new Document(
            folderId: _currentFolder.Id,
            createdTimestamp: createdTimestamp,
            lastModifiedTimestamp: createdTimestamp,
            name: documentName
        ));
        
        _currentFolder.Documents.Add(newDocumentDocument);
        _allDocuments.Add(newDocumentDocument);
        CurrentDocuments.Add(newDocumentDocument);
    }
}
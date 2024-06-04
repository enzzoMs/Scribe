using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
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

    private readonly Action<FolderSelectedEvent> _onFolderSelected;
    private Folder? _currentFolder;

    private List<Document> _allDocuments = [];
    private ObservableCollection<Document> _currentDocuments = [];

    private bool _onEditMode;
    
    public FolderDetailsViewModel(
        IEventAggregator eventAggregator, IRepository<Folder> foldersRepository, IRepository<Document> documentsRepository)
    {
        _eventAggregator = eventAggregator;
        _foldersRepository = foldersRepository;
        _documentsRepository = documentsRepository;
        
        _onFolderSelected = eventData => CurrentFolder = eventData.Folder;
        _eventAggregator.Subscribe(_onFolderSelected);

        EnterEditModeCommand = new DelegateCommand(_ => OnEditMode = true);
        ExitCurrentModeCommand = new DelegateCommand(_ => OnEditMode = false);
        UpdateFolderNameCommand = new DelegateCommand(param =>
        {
            if (param is not string newFolderName) return;
            OnEditMode = false;
            UpdateFolderName(newFolderName);
        });
        CreateDocumentCommand = new DelegateCommand(_ => CreateDocument());
    }
    
    public Folder? CurrentFolder
    {
        get => _currentFolder;
        set
        {
            _currentFolder = value;
            LoadDocuments();
            
            RaisePropertyChanged();
            RaisePropertyChanged(nameof(FolderNavigationPosition));
        }
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

    public int FolderNavigationPosition
    {
        get => _currentFolder?.NavigationIndex + 1 ?? 1;
        set => UpdateFolderPosition(value - 1);
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
    
    public ICommand EnterEditModeCommand { get; }

    public ICommand ExitCurrentModeCommand { get; }

    public ICommand CreateDocumentCommand { get; }

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

    private void LoadDocuments()
    {
        if (_currentFolder == null) return;
        
        _allDocuments = _currentFolder.Documents.ToList();
        CurrentDocuments = new ObservableCollection<Document>(_allDocuments);
    }

    private async void CreateDocument()
    {
        if (_currentFolder == null) return;
        //TODO
        //ClearFilter();
        
        var documentName = (string) Application.Current.TryFindResource("String.Documents.DefaultName") ?? "New Folder";

        var createdTimestamp = DateTime.Now;

        var newDocumentDocument = await _documentsRepository.Add(new Document(
            folderId: _currentFolder.Id,
            createdTimestamp: createdTimestamp,
            lastModifiedTimestamp: createdTimestamp,
            name: documentName
        ));
        
        _allDocuments.Add(newDocumentDocument);
        CurrentDocuments.Add(newDocumentDocument);
    }
}
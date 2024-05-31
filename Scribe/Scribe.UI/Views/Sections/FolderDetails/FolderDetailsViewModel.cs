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
    
    private readonly Action<FolderSelectedEvent> _onFolderSelected;
    private Folder? _currentFolder;

    private bool _onEditMode;
    
    public FolderDetailsViewModel(IEventAggregator eventAggregator, IRepository<Folder> foldersRepository)
    {
        _eventAggregator = eventAggregator;
        _foldersRepository = foldersRepository;
        
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
    }
    
    public Folder? CurrentFolder
    {
        get => _currentFolder;
        set
        {
            _currentFolder = value;
            RaisePropertyChanged();
            RaisePropertyChanged(nameof(FolderNavigationPosition));
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
}
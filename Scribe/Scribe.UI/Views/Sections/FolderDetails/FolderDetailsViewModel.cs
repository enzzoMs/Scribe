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

    private Folder? _folder;

    private bool _onEditMode;
    
    public FolderDetailsViewModel(IEventAggregator eventAggregator, IRepository<Folder> foldersRepository)
    {
        _eventAggregator = eventAggregator;
        _foldersRepository = foldersRepository;
        
        _eventAggregator.Subscribe<FolderSelectedEvent>(this, OnFolderSelected);

        EnterEditModeCommand = new DelegateCommand( _ => OnEditMode = true);
        UpdateFolderNameCommand = new DelegateCommand(param =>
        {
            if (param is not string newFolderName) return;
            OnEditMode = false;
            UpdateFolderName(newFolderName);
        });
        DeleteFolderCommand = new DelegateCommand(_ => DeleteFolder());
    }
    
    public Folder? Folder
    {
        get => _folder;
        set
        {
            _folder = value;
            OnEditMode = false;
            
            RaisePropertyChanged();
            RaisePropertyChanged(nameof(FolderNavigationPosition));
        }
    }
    
    public int FolderNavigationPosition
    {
        get => _folder?.NavigationIndex + 1 ?? 1;
        set => UpdateFolderPosition(value - 1);
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

    public ICommand UpdateFolderNameCommand { get; }

    public ICommand DeleteFolderCommand { get; }
    
    public ICommand EnterEditModeCommand { get; }

    private async void UpdateFolderName(string newFolderName)
    {
        if (_folder == null || _folder.Name == newFolderName) return;

        _folder.Name = newFolderName.Trim();
        await _foldersRepository.Update(_folder);
        
        RaisePropertyChanged(nameof(Folder));
        _eventAggregator.Publish(new FolderUpdatedEvent(_folder.Id));
    }

    private void UpdateFolderPosition(int newIndex)
    {
        if (_folder == null || _folder.NavigationIndex == newIndex) return;

        var oldIndex = _folder.NavigationIndex;
        
        _eventAggregator.Publish(new FolderPositionUpdatedEvent(_folder.Id, oldIndex, newIndex));
    }

    private async void DeleteFolder()
    {
        if (_folder == null) return;
        
        await _foldersRepository.Delete(_folder);
        
        _eventAggregator.Publish(new FolderDeletedEvent(_folder));
    }

    private void OnFolderSelected(FolderSelectedEvent folderEvent) => Folder = folderEvent.Folder;
}
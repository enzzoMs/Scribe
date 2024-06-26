using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Scribe.Data.Model;
using Scribe.Data.Repositories;
using Scribe.UI.Command;
using Scribe.UI.Events;
using Scribe.UI.Views.Sections.Configurations;

namespace Scribe.UI.Views.Sections.Navigation;

public class NavigationViewModel : BaseViewModel
{
    private readonly IEventAggregator _eventAggregator;
    private readonly IRepository<Folder> _foldersRepository;
    private readonly IRepository<Tag> _tagsRepository;
    
    private List<Folder> _allFolders = [];
    private ObservableCollection<Folder> _currentFolders = [];
    private Folder? _selectedFolder;

    private bool _isNavigationCollapsed;
    
    private readonly DispatcherTimer _searchTimer;
    private string _searchFoldersFilter = "";
    private const int SearchDelayMs = 800;
    
    public NavigationViewModel(
        IEventAggregator eventAggregator, 
        IRepository<Folder> foldersRepository,
        IRepository<Tag> tagsRepository,
        ConfigurationsViewModel configurationsViewModel)
    {
        _eventAggregator = eventAggregator;
        _foldersRepository = foldersRepository;
        _tagsRepository = tagsRepository;
        
        _searchTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(SearchDelayMs) };
        _searchTimer.Tick += (_, _) =>
        {
            _searchTimer.Stop();
            FilterFolders();
        };

        _eventAggregator.Subscribe<FolderUpdatedEvent>(this, OnFolderUpdated);
        _eventAggregator.Subscribe<FolderDeletedEvent>(this, OnFolderDeleted);
        _eventAggregator.Subscribe<TagAddedEvent>(this, OnTagAdded);
        _eventAggregator.Subscribe<TagRemovedEvent>(this, OnTagRemoved);

        ConfigurationsViewModel = configurationsViewModel;

        CreateFolderCommand = new DelegateCommand(_ => CreateFolder());
        CollapseNavigationCommand = new DelegateCommand(parameter =>
        {
            if (parameter is bool isCollapsed) IsNavigationCollapsed = isCollapsed;
        });
    }
    
    public ConfigurationsViewModel ConfigurationsViewModel { get; private set; }
    
    public ObservableCollection<Folder> CurrentFolders
    {
        get => _currentFolders;
        private set
        {
            _currentFolders = value;
            RaisePropertyChanged();
        }
    }

    public Folder? SelectedFolder
    {
        get => _selectedFolder;
        set
        {
            _selectedFolder = value;
            _eventAggregator.Publish(new FolderSelectedEvent(_selectedFolder));
        }
    }

    public string SearchFoldersFilter
    {
        get => _searchFoldersFilter;
        set
        {
            _searchFoldersFilter = value;

            if (DelayFoldersSearch)
            {
                _searchTimer.Stop();
                _searchTimer.Start();   
            }
            else
            {
                FilterFolders();
            }
        }
    }

    public bool IsNavigationCollapsed
    {
        get => _isNavigationCollapsed;
        set
        {
            _isNavigationCollapsed = value;
            RaisePropertyChanged();
        }
    }

    public bool DelayFoldersSearch { get; set; } = true;
    
    public ICommand CreateFolderCommand { get; private set; }
    
    public ICommand CollapseNavigationCommand { get; private set; }

    public void LoadFolders(IEnumerable<Folder> folders)
    {
        _allFolders = folders.OrderBy(folder => folder.NavigationIndex).ToList();
        CurrentFolders = new ObservableCollection<Folder>(_allFolders);
    }

    private void FilterFolders()
    {
        var filterText = _searchFoldersFilter.Trim();
        var filteredFolders = _allFolders.Where(folder => folder.Name.Contains(
            filterText, StringComparison.CurrentCultureIgnoreCase
        ));
        CurrentFolders = new ObservableCollection<Folder>(filteredFolders);
    }

    private void ShowAllFolders()
    {
        _searchFoldersFilter = "";
        RaisePropertyChanged(nameof(SearchFoldersFilter));
        CurrentFolders = new ObservableCollection<Folder>(_allFolders);
    }
    
    private async void CreateFolder()
    {
        var folderName = Application.Current?.TryFindResource("String.Folders.DefaultName") as string ?? "New Folder";
        
        var newFolder = await _foldersRepository.Add(new Folder(
            name: folderName,
            navigationIndex: _allFolders.Count
        ));
        
        _allFolders.Add(newFolder);
        ShowAllFolders();
    }

    private async void OnFolderDeleted(FolderDeletedEvent folderEvent)
    {
        var deletedFolder = folderEvent.DeletedFolder;
        
        _allFolders.Remove(deletedFolder);

        var foldersAfterDeletedFolder = _allFolders.Where(f => f.NavigationIndex > deletedFolder.NavigationIndex).ToArray();
        
        foreach (var folder in foldersAfterDeletedFolder)
        {
            folder.NavigationIndex--;
        }

        await _foldersRepository.Update(foldersAfterDeletedFolder);
        
        ShowAllFolders();
    }
    
    private async void OnFolderUpdated(FolderUpdatedEvent folderEvent)
    {
        if (folderEvent is FolderPositionUpdatedEvent positionEvent)
        {
            if (positionEvent.NewIndex >= _allFolders.Count) return;
            
            // Swapping folder positions

            var folderInOldPosition = _allFolders[positionEvent.OldIndex];

            var folderInNewPosition = _allFolders[positionEvent.NewIndex];
            
            folderInNewPosition.NavigationIndex = positionEvent.OldIndex;
            folderInOldPosition.NavigationIndex = positionEvent.NewIndex;

            await _foldersRepository.Update(folderInNewPosition);
            await _foldersRepository.Update(folderInOldPosition);

            (_allFolders[positionEvent.NewIndex], _allFolders[positionEvent.OldIndex]) = 
                (_allFolders[positionEvent.OldIndex], _allFolders[positionEvent.NewIndex]);
        }

        if (folderEvent.UpdatedFolderId == _selectedFolder?.Id)
        {
            RaisePropertyChanged(nameof(SelectedFolder));
        }
        
        ShowAllFolders();
    }

    private async void OnTagAdded(TagAddedEvent tagEvent)
    {
        var createdTag = tagEvent.CreatedTag;
        
        var associatedFolder = _allFolders.Find(f => f.Id == createdTag.FolderId);

        if (associatedFolder == null || associatedFolder.Tags.Any(tag => tag.Name == createdTag.Name)) return;

        associatedFolder.Tags.Add(createdTag);
        await _tagsRepository.Add(createdTag);
    }
    
    private async void OnTagRemoved(TagRemovedEvent tagEvent)
    {
        var removedTag = tagEvent.RemovedTag;
        
        var associatedFolder = _allFolders.Find(folder => folder.Id == removedTag.FolderId);

        if (associatedFolder == null) return;

        var tagIsStillUsed = associatedFolder.Documents.Any(doc => doc.Tags.Any(tag => tag.Name == removedTag.Name));

        if (tagIsStillUsed) return;
        
        associatedFolder.Tags.Remove(associatedFolder.Tags.First(tag => tag.Name == removedTag.Name));
        await _tagsRepository.Delete(removedTag);
    }
}
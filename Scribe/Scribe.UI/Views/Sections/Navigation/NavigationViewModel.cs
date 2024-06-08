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
        ConfigurationsViewModel configurationsViewModel)
    {
        _eventAggregator = eventAggregator;
        _foldersRepository = foldersRepository;
        
        _searchTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(SearchDelayMs) };
        _searchTimer.Tick += (_, _) =>
        {
            _searchTimer.Stop();
            FilterFolders();
        };

        _eventAggregator.Subscribe<FolderUpdatedEvent>(this, OnFolderUpdated);
        
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
            _searchTimer.Stop();
            _searchTimer.Start();
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

    private void ClearFoldersFilter()
    {
        _searchFoldersFilter = "";
        RaisePropertyChanged(nameof(SearchFoldersFilter));
        FilterFolders();
    }
    
    private async void CreateFolder()
    {
        ClearFoldersFilter();
        
        var folderName = (string) Application.Current.TryFindResource("String.Folders.DefaultName") ?? "New Folder";
        
        var newFolder = await _foldersRepository.Add(new Folder(
            name: folderName,
            navigationIndex: _allFolders.Count
        ));
        
        _allFolders.Add(newFolder);
        CurrentFolders.Add(newFolder);
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
            
            ClearFoldersFilter();
        }
        
        if (folderEvent.UpdatedFolderId == _selectedFolder?.Id) 
            RaisePropertyChanged(nameof(SelectedFolder));
    }
}
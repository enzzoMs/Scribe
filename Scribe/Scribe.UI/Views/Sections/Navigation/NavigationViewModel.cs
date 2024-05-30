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

    private Action<FolderUpdatedEvent> _onFolderUpdated;
    
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

        _onFolderUpdated = _ => RaisePropertyChanged(nameof(SelectedFolder));
        _eventAggregator.Subscribe(_onFolderUpdated);
        
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
        _allFolders = folders.OrderBy(folder => folder.NavigationPosition).ToList();
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
    
    private async void CreateFolder()
    {
        _searchFoldersFilter = "";
        RaisePropertyChanged(nameof(SearchFoldersFilter));
        FilterFolders();
        
        var folderName = (string) Application.Current.TryFindResource("String.Folders.DefaultName") ?? "New Folder";
        
        var newFolder = await _foldersRepository.Add(new Folder(
            name: folderName,
            navigationPosition: _allFolders.Count + 1
        ));
        
        _allFolders.Insert(0, newFolder);
        CurrentFolders.Insert(0, newFolder);
    }
}
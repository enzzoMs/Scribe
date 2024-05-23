using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Scribe.Data.Model;
using Scribe.Data.Repositories;
using Scribe.UI.Command;
using Scribe.UI.Views.Sections.Configurations;

namespace Scribe.UI.Views.Sections.Folders;

public class FoldersViewModel : BaseViewModel
{    
    private readonly IRepository<Folder> _foldersRepository;
    
    private List<Folder> _allFolders = [];
    private ObservableCollection<Folder> _currentFolders = [];
    
    private readonly DispatcherTimer _searchTimer;
    private string _searchFoldersFilter = "";
    private const int SearchDelayMs = 800;
    
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
    public ICommand CreateFolderCommand { get; private set; }
    

    public FoldersViewModel(IRepository<Folder> foldersRepository, ConfigurationsViewModel configurationsViewModel)
    {
        ConfigurationsViewModel = configurationsViewModel;
        
        _foldersRepository = foldersRepository;
        CreateFolderCommand = new DelegateCommand(_ => CreateFolder());
        
        _searchTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(SearchDelayMs) };
        _searchTimer.Tick += (_, _) =>
        {
            _searchTimer.Stop();
            FilterFolders();
        };
    }

    public void LoadFolders(IEnumerable<Folder> folders)
    {
        _allFolders = folders.OrderBy(folder => folder.Index).ToList();
        CurrentFolders = new ObservableCollection<Folder>(_allFolders);
    }

    private void FilterFolders()
    {
        var filterText = _searchFoldersFilter.Trim();
        var filteredFolders = _allFolders.Where(folder => folder.Name.Contains(
            filterText, StringComparison.CurrentCultureIgnoreCase)
        );
        CurrentFolders = new ObservableCollection<Folder>(filteredFolders);
    }
    
    private async void CreateFolder()
    {
        var folderName = (string) Application.Current.TryFindResource("String.Folders.DefaultName") ?? "New Folder";
        
        var newFolder = await _foldersRepository.Add(new Folder(folderName, _allFolders.Count + 1));
        
        _allFolders.Insert(0, newFolder);
        CurrentFolders.Insert(0, newFolder);
    }
}
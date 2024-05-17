using System.Collections.ObjectModel;
using System.Windows.Input;
using Scribe.Data.Model;
using Scribe.Data.Repositories;
using Scribe.UI.Command;

namespace Scribe.UI.Views.Folders;

public class FoldersViewModel : BaseViewModel
{    
    private readonly IRepository<Folder> _foldersRepository;
    public ICommand AddFolderCommand { get; private set; }
    public ObservableCollection<Folder> Folders { get; private set; } = [];

    public FoldersViewModel(IRepository<Folder> foldersRepository)
    {
        _foldersRepository = foldersRepository;
        AddFolderCommand = new DelegateCommand(AddFolder);
    }

    public void LoadFolders(IEnumerable<Folder> folders) => Folders = new ObservableCollection<Folder>(
        folders.OrderBy(folder => folder.Index)
    );
    
    private async void AddFolder(object? parameter)
    {
        var newFolder = new Folder("Nova Pasta", Folders.Count + 1);
        Folders.Insert(0, await _foldersRepository.Add(newFolder));
    }
}
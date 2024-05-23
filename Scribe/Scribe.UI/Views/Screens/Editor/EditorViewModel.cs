using Scribe.Data.Model;
using Scribe.UI.Views.Sections.Folders;

namespace Scribe.UI.Views.Screens.Editor;

public class EditorViewModel(FoldersViewModel foldersViewModel) : BaseViewModel
{
    public FoldersViewModel FoldersViewModel { get; } = foldersViewModel;

    public void LoadFolders(IEnumerable<Folder> folders) => FoldersViewModel.LoadFolders(folders);
}
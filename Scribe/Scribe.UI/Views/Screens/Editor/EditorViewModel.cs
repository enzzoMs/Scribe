using Scribe.Data.Model;
using Scribe.UI.Views.Sections.FolderDetails;
using Scribe.UI.Views.Sections.Navigation;

namespace Scribe.UI.Views.Screens.Editor;

public class EditorViewModel(
    NavigationViewModel navigationViewModel, FolderDetailsViewModel folderDetailsViewModel
) : BaseViewModel
{
    public NavigationViewModel NavigationViewModel { get; } = navigationViewModel;
    public FolderDetailsViewModel FolderDetailsViewModel { get; } = folderDetailsViewModel;

    public void LoadFolders(IEnumerable<Folder> folders) => NavigationViewModel.LoadFolders(folders);
}
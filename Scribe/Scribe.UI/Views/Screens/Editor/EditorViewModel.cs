using Scribe.Data.Model;
using Scribe.UI.Views.Sections.Navigation;

namespace Scribe.UI.Views.Screens.Editor;

public class EditorViewModel(NavigationViewModel navigationViewModel) : BaseViewModel
{
    public NavigationViewModel NavigationViewModel { get; } = navigationViewModel;

    public void LoadFolders(IEnumerable<Folder> folders) => NavigationViewModel.LoadFolders(folders);
}
﻿using Scribe.Data.Model;
using Scribe.UI.Views.Sections.Editor;
using Scribe.UI.Views.Sections.FolderDetails;
using Scribe.UI.Views.Sections.Navigation;

namespace Scribe.UI.Views.Screens.Main;

public class MainViewModel(
    NavigationViewModel navigationViewModel,
    FolderDetailsViewModel folderDetailsViewModel, 
    EditorViewModel editorViewModel
) : BaseViewModel
{
    public NavigationViewModel NavigationViewModel { get; } = navigationViewModel;
    public FolderDetailsViewModel FolderDetailsViewModel { get; } = folderDetailsViewModel;
    
    public EditorViewModel EditorViewModel { get; } = editorViewModel;

    public void LoadFolders(IEnumerable<Folder> folders) => NavigationViewModel.LoadFolders(folders);
}
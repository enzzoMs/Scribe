﻿using Scribe.Data.Model;
using Scribe.UI.Views.Sections.Documents;
using Scribe.UI.Views.Sections.Editor;
using Scribe.UI.Views.Sections.FolderDetails;
using Scribe.UI.Views.Sections.Navigation;
using Scribe.UI.Views.Sections.Tags;

namespace Scribe.UI.Views.Screens.Main;

public class MainViewModel(
    NavigationViewModel navigationViewModel,
    FolderDetailsViewModel folderDetailsViewModel,
    DocumentsViewModel documentsViewModel,
    TagsViewModel tagsViewModel,
    EditorViewModel editorViewModel
) : BaseViewModel
{
    public NavigationViewModel NavigationViewModel { get; } = navigationViewModel;
    public FolderDetailsViewModel FolderDetailsViewModel { get; } = folderDetailsViewModel;
    
    public DocumentsViewModel DocumentsViewModel { get; } = documentsViewModel;
    
    public TagsViewModel TagsViewModel { get; } = tagsViewModel;

    public EditorViewModel EditorViewModel { get; } = editorViewModel;

    public void LoadFolders(IEnumerable<Folder> folders) => NavigationViewModel.LoadFolders(folders);
}
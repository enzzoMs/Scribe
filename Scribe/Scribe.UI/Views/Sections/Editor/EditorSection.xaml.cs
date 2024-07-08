using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Scribe.UI.Views.Components;
using Scribe.UI.Views.Sections.Editor.State;
using MessageBox = Scribe.UI.Views.Components.MessageBox;

namespace Scribe.UI.Views.Sections.Editor;

public partial class EditorSection : UserControl
{
    public EditorSection()
    {
        InitializeComponent();

        if (Application.Current.MainWindow == null) return;
        
        Application.Current.MainWindow.Closing += (_, _) =>
        {
            EditorTabControl.CloseAllTabs();
        };
    }

    private void OnAddTagButtonClicked(object sender, RoutedEventArgs e)
    {
        var button = (Button) sender;
        var contextMenu = button.ContextMenu;
        
        if (contextMenu != null)
        {
            contextMenu.PlacementTarget = button;
            contextMenu.Placement = PlacementMode.Bottom;
            contextMenu.IsOpen = true;
        }
    }

    private void OnConfirmTagNameClicked(object sender, RoutedEventArgs e)
    {
        var iconButton = (IconButton) sender;
        var stackPanel = (StackPanel) iconButton.Parent;
        var menuItem = (MenuItem) stackPanel.TemplatedParent;
        var contextMenu = (ContextMenu) menuItem.Parent;
        contextMenu.IsOpen = false;
    }

    private void OnDocumentContentChanged(object? sender, string e)
    {
        var documentState = ((EditorViewModel) DataContext).SelectedDocument;
        
        if (documentState != null && documentState.EditedContent != documentState.Document.Content)
        {
            documentState.HasUnsavedChanges = true;
        }
    }

    private void OnCloseTabClicked(object? sender, object e)
    {
        var documentState = (DocumentViewState) e;
        var editorViewModel = (EditorViewModel) DataContext;
        var appResources = Application.Current.Resources;

        if (documentState.HasUnsavedChanges)
        {
            var saveDocumentMessage = string.Format(
                appResources["String.Documents.Save.Message"] as string ?? "", 
                documentState.Document.Name
            );
            
            new MessageBox
            {
                Owner = Application.Current.MainWindow,
                Title = appResources["String.Documents.Save"] as string,
                Message = saveDocumentMessage,
                Options = [
                    new MessageBoxOption(
                        appResources["String.Button.Save"] as string ?? "",
                        editorViewModel.SaveAndCloseDocumentCommand,
                        documentState
                    ),
                    new MessageBoxOption(
                        appResources["String.Button.DontSave"] as string ?? "", 
                        editorViewModel.CloseDocumentCommand,
                        documentState
                    )
                ]
            }.ShowDialog();
        }
        else
        {
            editorViewModel.CloseDocumentCommand.Execute(documentState);
        }
    }
}
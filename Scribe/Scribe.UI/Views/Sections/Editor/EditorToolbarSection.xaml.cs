using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Scribe.UI.Views.Components;

namespace Scribe.UI.Views.Sections.Editor;

public partial class EditorToolbarSection : UserControl
{
    public EditorToolbarSection() => InitializeComponent();

    private void OnDeleteButtonClicked(object sender, MouseButtonEventArgs e)
    {
        var editorViewModel = (EditorViewModel) DataContext;
        var appResources = Application.Current.Resources;
        
        var confirmationMessage = string.Format(
            appResources["String.Documents.Delete.Message"] as string ?? "", 
            editorViewModel.SelectedDocument?.Name
        );
        
        new ConfirmationMessageBox
        {
            Owner = Application.Current.MainWindow,
            WindowTitle = appResources["String.Documents.Delete"] as string ?? "",
            ConfirmationMessage = confirmationMessage,
            OnConfirm = editorViewModel.DeleteDocumentCommand
        }.ShowDialog();
    }
}
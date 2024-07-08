using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Scribe.UI.Views.Components;
using MessageBox = Scribe.UI.Views.Components.MessageBox;

namespace Scribe.UI.Views.Sections.Editor;

public partial class EditorToolbarSection : UserControl
{
    public EditorToolbarSection() => InitializeComponent();

    private void OnDeleteDocumentClicked(object sender, MouseButtonEventArgs e)
    {
        var editorViewModel = (EditorViewModel) DataContext;
        var appResources = Application.Current.Resources;
        
        var boxMessage = string.Format(
            appResources["String.Documents.Delete.Message"] as string ?? "", 
            editorViewModel.SelectedDocument?.Document.Name
        );
        
        new MessageBox
        {
            Owner = Application.Current.MainWindow,
            Title = appResources["String.Documents.Delete"] as string ?? "",
            Message = boxMessage,
            Options = [
                new MessageBoxOption(
                    appResources["String.Button.Delete"] as string ?? "", 
                    editorViewModel.DeleteSelectedDocumentCommand
                ),
                new MessageBoxOption(appResources["String.Button.Cancel"] as string ?? "")
            ]
        }.ShowDialog();
    }
}
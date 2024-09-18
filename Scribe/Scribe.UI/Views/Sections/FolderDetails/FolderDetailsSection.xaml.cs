using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Scribe.UI.Views.Components;
using MessageBox = Scribe.UI.Views.Components.MessageBox;

namespace Scribe.UI.Views.Sections.FolderDetails;

public partial class FolderDetailsSection : UserControl
{
    public FolderDetailsSection() => InitializeComponent();
    
    private void OnDeleteFolderClicked(object sender, MouseButtonEventArgs e)
    {
        var detailsViewModel = (FolderDetailsViewModel) DataContext;
        var appResources = Application.Current.Resources;
        
        string boxMessage;

        if (detailsViewModel.Folder?.Documents.Count == 0)
        {
            boxMessage = string.Format(
                appResources["String.Folders.Delete.Message"] as string ?? "", 
                detailsViewModel.Folder.Name
            );
        }
        else
        {
            var deleteMessage = string.Format(
                appResources["String.Folders.Delete.Message"] as string ?? "",
                detailsViewModel.Folder?.Name
            );

            var documentsWarningMessage = string.Format(
                appResources["String.Folders.Delete.DocumentsWarning"] as string ?? "",
                detailsViewModel.Folder?.Documents.Count
            );
            
            boxMessage = $"{deleteMessage}\n{documentsWarningMessage}";
        }
        
        new MessageBox
        {
            Owner = Application.Current.MainWindow,
            Title = appResources["String.Folders.Delete"] as string ?? "",
            MessageIconPath = appResources["Drawing.QuestionMark"] as Geometry,
            Message = boxMessage,
            Options = [
                new MessageBoxOption(appResources["String.Button.Delete"] as string ?? "", detailsViewModel.DeleteFolderCommand),
                new MessageBoxOption(appResources["String.Button.Cancel"] as string ?? "")
            ]
        }.ShowDialog();
    }
}
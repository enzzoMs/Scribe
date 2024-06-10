using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Scribe.UI.Views.Components;

namespace Scribe.UI.Views.Sections.FolderDetails;

public partial class FolderNameSection : UserControl
{
    public FolderNameSection() => InitializeComponent();

    private void OnDeleteButtonClicked(object sender, MouseButtonEventArgs e)
    {
        var detailsViewModel = (FolderDetailsViewModel) DataContext;
        var appResources = Application.Current.Resources;
        
        string confirmationMessage;

        if (detailsViewModel.CurrentFolder?.Documents.Count == 0)
        {
            confirmationMessage = string.Format(
                appResources["String.Folders.Delete.Message"] as string ?? "", 
                detailsViewModel.CurrentFolder.Name
            );
        }
        else
        {
            var deleteMessage = string.Format(
                appResources["String.Folders.Delete.Message"] as string ?? "",
                detailsViewModel.CurrentFolder?.Name
            );

            var documentsWarningMessage = string.Format(
                appResources["String.Folders.Delete.DocumentsWarning"] as string ?? "",
                detailsViewModel.CurrentFolder?.Documents.Count
            );
            
            confirmationMessage = $"{deleteMessage}\n{documentsWarningMessage}";
        }
        
        new ConfirmationMessageBox
        {
            Owner = Application.Current.MainWindow,
            WindowTitle = appResources["String.Folders.Delete"] as string ?? "",
            ConfirmationMessage = confirmationMessage,
            OnConfirm = detailsViewModel.DeleteFolderCommand
        }.ShowDialog();
    }
}
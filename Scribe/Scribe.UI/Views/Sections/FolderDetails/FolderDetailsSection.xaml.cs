using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Scribe.Data.Model;
using Scribe.UI.Commands;
using Scribe.UI.Views.Components;
using Scribe.UI.Views.Errors;
using MessageBox = Scribe.UI.Views.Components.MessageBox;

namespace Scribe.UI.Views.Sections.FolderDetails;

public partial class FolderDetailsSection : UserControl
{
    private readonly ICommand _exportFolderProxyCommand;
    
    public FolderDetailsSection()
    {
        InitializeComponent();
        
        _exportFolderProxyCommand = new DelegateCommand(param =>
        {
            if (param is not ValueTuple<string, string> (object chosenOption, var directoryPath)) return;

            var documentFileFormat = (string) chosenOption;
            
            if (documentFileFormat == DocumentFileFormats.Json.ToString())
            {
                ((FolderDetailsViewModel) DataContext).ExportFolder(directoryPath);
            }
        });
    }

    private void OnFolderDetailsSectionLoaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is FolderDetailsViewModel folderDetailsViewModel)
        {
            folderDetailsViewModel.ViewModelError += OnViewModelError;
        }
    }

    private static void OnViewModelError(object? sender, IViewModelError error)
    {
        var appResources = Application.Current.Resources;

        var errorMessage = error switch
        {
            DocumentExportError => appResources["String.Error.ExportDocument"] as string ?? "",
            DocumentImportFilePathError filePathError => string.Format(
                appResources["String.Error.ImportPathError"] as string ?? "", filePathError.FilePath 
            ),
            DocumentImportFileFormatError fileFormatError => string.Format(
                appResources["String.Error.ImportFileFormatError"] as string ?? "", fileFormatError.FilePath
            ),
            _ => null
        };
        
        if (errorMessage != null)
        {
            new MessageBox
            {
                Owner = Application.Current.MainWindow,
                Title = appResources["String.Error"] as string,
                Message = errorMessage,
                MessageIconPath = appResources["Drawing.Exclamation"] as Geometry,
                Options = [new MessageBoxOption(appResources["String.Button.Understood"] as string ?? "")]
            }.ShowDialog();
        }
    }

    private void OnDeleteFolderClicked(object sender, RoutedEventArgs e)
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
    
    private void OnFolderActionsButtonClicked(object sender, MouseButtonEventArgs e)
    {
        var contextMenu = FolderActionsButton.ContextMenu;
        
        if (contextMenu == null) return;
        
        contextMenu.PlacementTarget = FolderActionsButton;
        contextMenu.IsOpen = true;
    }

    private void OnExportFolderClicked(object sender, RoutedEventArgs e)
    {
        var appResources = Application.Current.Resources;
        var exportMessage = appResources["String.Button.Export"] as string;
        
        new PathChooserBox
        {
            Owner = Application.Current.MainWindow,
            Title = exportMessage,
            Options = [DocumentFileFormats.Json],
            ConfirmActionMessage = exportMessage,
            ConfirmActionCommand = _exportFolderProxyCommand
        }.ShowDialog();
    }

    private void OnImportDocumentsClicked(object sender, RoutedEventArgs e)
    {
        var folderDetailsViewModel = (FolderDetailsViewModel) DataContext;
        var documentFileFormat = FolderDetailsViewModel.DocumentImportFileFormat.ToString();
        
        new FileChooserBox
        {
            Title = Application.Current.Resources["String.Button.Import"] as string,
            FileFilter = $"{documentFileFormat} documents (.{documentFileFormat.ToLower()})|*.{documentFileFormat.ToLower()}",
            ConfirmActionCommand = folderDetailsViewModel.ImportDocumentsCommand
        }.ShowDialog();
    }
}
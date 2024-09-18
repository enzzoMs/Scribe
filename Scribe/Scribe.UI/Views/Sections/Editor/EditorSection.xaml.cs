using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Scribe.UI.Command;
using Scribe.UI.Views.Components;
using Scribe.UI.Views.Sections.Editor.State;
using MessageBox = Scribe.UI.Views.Components.MessageBox;

namespace Scribe.UI.Views.Sections.Editor;

public partial class EditorSection : UserControl
{
    public EditorSection()
    {
        InitializeComponent();

        if (Application.Current.MainWindow != null)
        {
            Application.Current.MainWindow.Closing += (_, _) => { EditorTabControl.CloseAllTabs(); };
        }
        
        UpdateDocumentNameProxyCommand = new DelegateCommand(param =>
        {
            ((EditorViewModel) DataContext).UpdateSelectedDocumentNameCommand.Execute(param);
            EditorTabControl.UpdateSelectedTabHeader();
        });
    }

    public ICommand UpdateDocumentNameProxyCommand { get; }

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
                MessageIconPath = appResources["Drawing.QuestionMark"] as Geometry,
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
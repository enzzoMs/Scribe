using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Scribe.UI.Views.Components;
using MessageBox = Scribe.UI.Views.Components.MessageBox;

namespace Scribe.UI.Views.Sections.Editor;

public partial class EditorBody : UserControl
{
    public EditorBody()
    {
        InitializeComponent();

        if (MarkupEditor.FindName("EditorTextBox") is not TextBox editorTextBox) return;
        
        UndoDocumentEditButton.Command = ApplicationCommands.Undo;
        UndoDocumentEditButton.CommandTarget = editorTextBox;
        
        RedoDocumentEditButton.Command = ApplicationCommands.Redo;
        RedoDocumentEditButton.CommandTarget = editorTextBox;
    }

    public object Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }
    
    public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(
        name: nameof(Header),
        propertyType: typeof(object),
        ownerType: typeof(EditorBody)
    );
    
    private void OnDocumentContentChanged(object? sender, string e)
    {
        var documentState = ((EditorViewModel) DataContext).SelectedDocument;
        
        if (documentState != null && documentState.EditedContent != documentState.Document.Content)
        {
            documentState.HasUnsavedChanges = true;
        }
    }
    
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

    private void OnMarkupIconClicked(object sender, RoutedEventArgs e)
    {
        var nodeType = (Type) ((IconButton) sender).CommandParameter;
        MarkupEditor.InsertMarkupNode(nodeType);
    }
}
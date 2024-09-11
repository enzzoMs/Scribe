using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ICSharpCode.AvalonEdit;
using Scribe.UI.Views.Components;
using MessageBox = Scribe.UI.Views.Components.MessageBox;

namespace Scribe.UI.Views.Sections.Editor;

public partial class EditorBody : UserControl
{
    public EditorBody()
    {
        InitializeComponent();
        UpdateUndoRedoButtons();
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
    
    private void UpdateUndoRedoButtons()
    {
        if (MarkupEditor.FindName("EditorTextBox") is not TextEditor editorTextBox) return;

        UndoDocumentEditButton.IsEnabled = editorTextBox.CanUndo;
        RedoDocumentEditButton.IsEnabled = editorTextBox.CanRedo;
    }
    
    private void OnUndoButtonClicked(object sender, RoutedEventArgs e)
    {
        if (MarkupEditor.FindName("EditorTextBox") is not TextEditor editorTextBox) return;

        editorTextBox.Undo();
        UpdateUndoRedoButtons();
    }

    private void OnRedoButtonClicked(object sender, RoutedEventArgs e)
    {
        if (MarkupEditor.FindName("EditorTextBox") is not TextEditor editorTextBox) return;

        editorTextBox.Redo();
        UpdateUndoRedoButtons();
    }

    private void OnDocumentContentChanged(object? sender, string e)
    {
        var documentState = ((EditorViewModel) DataContext).SelectedDocument;

        if (documentState == null) return;
        
        documentState.EditedContent = e;
         
        if (documentState.EditedContent != documentState.Document.Content)
        {
            documentState.HasUnsavedChanges = true;
        }
        
        UpdateUndoRedoButtons();
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
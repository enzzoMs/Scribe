using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ICSharpCode.AvalonEdit;
using Scribe.Markup.Inlines;
using Scribe.UI.Views.Components;
using Color = System.Drawing.Color;
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
            MessageIconPath = appResources["Drawing.QuestionMark"] as Geometry,
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
        var markupIcon = (IconButton) sender;

        if (markupIcon.CommandParameter is InlineMarkupModifiers inlineModifier)
        {
            MarkupEditor.InsertInlineModifier(inlineModifier);
        }
        else if (markupIcon.CommandParameter is Type markupType)
        {
            if (markupType == typeof(Uri))
            {
                MarkupEditor.InsertLinkModifier();
            }
            else if (markupType == typeof(Color))
            {
                MarkupEditor.InsertColorModifier();
            }
            else
            {
                MarkupEditor.InsertBlockNode(markupType);
            }
        }
    }

    private void OnToolbarSizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (!e.WidthChanged) return;
        
        var newWidth = (int) e.NewSize.Width;

        const int markupIconButtonSize = 36;
        
        var numOfIcons = newWidth / markupIconButtonSize;
        numOfIcons = numOfIcons < 0 ? 0 : numOfIcons;

        foreach (var view in MarkupIconsGrid.Children)
        {
            if (view is not IconButton iconButton) continue;

            if (numOfIcons > 0)
            {
                iconButton.Visibility = Visibility.Visible;
                numOfIcons--;
            }
            else
            {
                iconButton.Visibility = Visibility.Collapsed;
            }
        }
    }
}
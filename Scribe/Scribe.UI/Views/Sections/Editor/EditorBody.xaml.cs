using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ICSharpCode.AvalonEdit;
using Scribe.Data.Model;
using Scribe.Markup.Inlines;
using Scribe.UI.Commands;
using Scribe.UI.Views.Components;
using Scribe.UI.Views.Errors;
using Color = System.Drawing.Color;
using MessageBox = Scribe.UI.Views.Components.MessageBox;

namespace Scribe.UI.Views.Sections.Editor;

public partial class EditorBody : UserControl
{
    private readonly ICommand _exportDocumentProxyCommand;
    
    public EditorBody()
    {
        InitializeComponent();
        UpdateUndoRedoButtons();

        _exportDocumentProxyCommand = new DelegateCommand(param =>
        {
            if (param is not ValueTuple<string, string> (object chosenOption, var directoryPath)) return;

            var documentFileFormat = (string) chosenOption;
            
            if (documentFileFormat == DocumentFileFormats.Json.ToString())
            {
                ((EditorViewModel) DataContext).ExportDocumentAsJson(directoryPath);
            } 
            else if (documentFileFormat == DocumentFileFormats.Pdf.ToString())
            {
                ((EditorViewModel) DataContext).ExportDocumentAsPdf(directoryPath, MarkupEditor.GetMarkupAsImage());
            }
        });
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
    
    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is EditorViewModel editorViewModel)
        {
            editorViewModel.ViewModelError += OnViewModelError;
        }
    }

    private static void OnViewModelError(object? sender, IViewModelError error)
    {
        var appResources = Application.Current.Resources;
        
        if (error is not DocumentExportError) return;
        
        new MessageBox
        {
            Owner = Application.Current.MainWindow,
            Title = appResources["String.Error"] as string,
            MessageIconPath = appResources["Drawing.Exclamation"] as Geometry,
            Message = appResources["String.Error.ExportDocument"] as string ?? "",
            Options = [new MessageBoxOption(appResources["String.Button.Understood"] as string ?? "")]
        }.ShowDialog();
    }
    
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

    private void OnExportDocumentClicked(object sender, RoutedEventArgs e)
    {
        var appResources = Application.Current.Resources;
        var exportMessage = appResources["String.Button.Export"] as string;
        
        new PathChooserBox
        {
            Owner = Application.Current.MainWindow,
            Title = exportMessage,
            Options = Enum.GetNames<DocumentFileFormats>().Cast<object>().ToList(),
            ConfirmActionMessage = exportMessage,
            ConfirmActionCommand = _exportDocumentProxyCommand
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
            if (markupType == typeof(Color))
            {
                MarkupEditor.InsertColorModifier();
            }
            else if (markupType == typeof(Uri))
            {
                MarkupEditor.InsertLinkModifier();
            }
            else
            {
                MarkupEditor.InsertBlockNode(markupType);
            }
        }
    }

    private void OnHelpIconClicked(object sender, RoutedEventArgs eventArgs)
    {
        if (DataContext is not EditorViewModel editorViewModel) return;

        var currentLanguage = editorViewModel.CurrentLanguage;
        var tutorialUri = new Uri($"/Resources/Tutorials/Tutorial_{currentLanguage}.txt", UriKind.Relative);

        string tutorialText;
        
        try
        {
            var tutorialResourceStream = Application.GetResourceStream(tutorialUri);

            if (tutorialResourceStream == null)
            {
                ShowTutorialErrorMessage();
                return;
            }
            
            using var reader = new StreamReader(tutorialResourceStream.Stream);
            tutorialText = reader.ReadToEnd();
        }
        catch (Exception e) when (e is IOException or FormatException)
        {
            ShowTutorialErrorMessage();
            return;
        }
        
        editorViewModel.InPreviewMode = true;
        MarkupEditor.RenderTextAsMarkup(tutorialText);
    }

    private void ShowTutorialErrorMessage()
    {
        var appResources = Application.Current.Resources;
        
        new MessageBox
        {
            Owner = Application.Current.MainWindow,
            Title = appResources["String.Error"] as string ?? "",
            MessageIconPath = appResources["Drawing.Exclamation"] as Geometry,
            Message = appResources["String.Error.Tutorial"] as string ?? "",
            Options = [new MessageBoxOption(appResources["String.Button.Understood"] as string ?? "")]
        }.ShowDialog();
    }

    private void OnToolbarSizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (!e.WidthChanged) return;
        
        // Showing or hiding toolbar icons based on the available space

        var newWidth = (int) e.NewSize.Width;

        if (MarkupIconsPanel.Children[0] is not IconButton sampleIcon) return;
        
        var iconSizeWithMargins = (int) (sampleIcon.ActualWidth + (sampleIcon.Margin.Right * 2));
        
        var numOfIcons = newWidth / iconSizeWithMargins;
        var remainingWidth = newWidth % iconSizeWithMargins;

        if (remainingWidth > sampleIcon.ActualWidth)
        {
            numOfIcons++;
        }
        
        numOfIcons = numOfIcons < 1 ? 1 : numOfIcons;
        
        foreach (var view in MarkupIconsPanel.Children)
        {
            if (view is not IconButton iconButton) continue;

            if (numOfIcons > 0)
            {
                iconButton.Visibility = Visibility.Visible;
                numOfIcons--;
            }
            else
            {
                iconButton.Visibility = Visibility.Hidden;
            }
        }
    }
}
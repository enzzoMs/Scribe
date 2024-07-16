using System.Text;
using System.Windows;
using System.Windows.Controls;
using Scribe.Markdown;
using Scribe.Markdown.Nodes;

namespace Scribe.UI.Views.Components;

public partial class MarkdownEditor : UserControl
{
    private const int LineStartToleranceInChars = 5;
    private int _lineCount = 1;
    
    private bool _editorTextBoxIsFocused;
    
    public event EventHandler<string>? EditorTextChanged;
    
    public MarkdownEditor()
    {
        InitializeComponent();

        EditorTextBox.GotFocus += (_, _) => { _editorTextBoxIsFocused = true; };
        EditorTextBox.LostFocus += (_, _) => { _editorTextBoxIsFocused = false; };
    }
    
    public string EditorText
    {
        get => (string) GetValue(EditorTextProperty);
        set => SetValue(EditorTextProperty, value);
    }
    
    public bool InPreviewMode
    {
        get => (bool) GetValue(InPreviewModeProperty);
        set => SetValue(InPreviewModeProperty, value);
    }
    
    public static readonly DependencyProperty EditorTextProperty = DependencyProperty.Register(
        name: nameof(EditorText),
        propertyType: typeof(string),
        ownerType: typeof(MarkdownEditor),
        typeMetadata: new FrameworkPropertyMetadata(propertyChangedCallback: OnMarkdownChanged)
    );

    public static readonly DependencyProperty InPreviewModeProperty = DependencyProperty.Register(
        name: nameof(InPreviewMode),
        propertyType: typeof(bool),
        ownerType: typeof(MarkdownEditor),
        typeMetadata: new FrameworkPropertyMetadata(
            defaultValue: false, 
            flags: FrameworkPropertyMetadataOptions.AffectsRender,
            propertyChangedCallback: OnMarkdownChanged
        )
    );
    
    private static void OnMarkdownChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var markdownEditor = (MarkdownEditor) d;
        
        if (markdownEditor.InPreviewMode)
        {
            RenderMarkdown(MarkdownParser.Parse(markdownEditor.EditorText), markdownEditor.MarkdownViewerPanel);
        }
    }

    private static void RenderMarkdown(DocumentNode documentRoot, Panel markdownViewerPanel)
    {
        markdownViewerPanel.Children.Clear();
        
        foreach (var node in documentRoot.Children)
        {
            if (node is ParagraphNode paragraphNode)
            {
                var paragraphTextBlock = new TextBlock
                {
                    Text = paragraphNode.Text, 
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(0, markdownViewerPanel.Children.Count == 0 ? 0 : 6, 0, 6)
                };
                
                markdownViewerPanel.Children.Add(paragraphTextBlock);
            }
        }
    }
    
    private void EditorTextChanged_OnTextChanged(object sender, TextChangedEventArgs e)
    {
        if (EditorTextBox.LineCount == -1) return;
        
        // If lines were removed
        if (EditorTextBox.LineCount < _lineCount)
        {
            _lineCount = EditorTextBox.LineCount;

            var lastLineNumberIndex = EditorLineNumbers.Text.IndexOf($"\n{_lineCount + 1}", StringComparison.Ordinal);
            
            if (lastLineNumberIndex == -1) return;
            
            // Remove the extra line numbers
            EditorLineNumbers.Text = EditorLineNumbers.Text[..lastLineNumberIndex];
        }
        // If lines were added
        else if (EditorTextBox.LineCount > _lineCount)
        {
            var lineNumbersText = new StringBuilder(EditorLineNumbers.Text);
                
            for (var lineNumber = _lineCount + 1; lineNumber <= EditorTextBox.LineCount; lineNumber++)
            {
                lineNumbersText.Append('\n');
                lineNumbersText.Append(lineNumber);
            }

            EditorLineNumbers.Text = lineNumbersText.ToString();
            _lineCount = EditorTextBox.LineCount;
        }
            
        var largestLineLength = EditorTextBox.Text.Split("\n").Max(line => line.Length);
        var currentLine = EditorTextBox.GetLineIndexFromCharacterIndex(EditorTextBox.CaretIndex);
        
        if (currentLine == -1) return;
        
        var currentLineStartIndex = EditorTextBox.GetCharacterIndexFromLineIndex(currentLine);

        // Scroll to the right end if the caret is at the end of the longest line.
        // This ensures that the text does not remain hidden behind the vertical scrollbar.
        if (EditorTextBox.CaretIndex - currentLineStartIndex >= largestLineLength - 1)
        {
            EditorScrollViewer.ScrollToRightEnd();
        }
        
        EditorTextChanged?.Invoke(this, EditorTextBox.Text);
    }

    private void EditorTextBox_OnSelectionChanged(object sender, RoutedEventArgs e)
    {
        var currentLine = EditorTextBox.GetLineIndexFromCharacterIndex(EditorTextBox.CaretIndex);
        
        if (currentLine == -1) return;
        
        var currentLineStartIndex = EditorTextBox.GetCharacterIndexFromLineIndex(currentLine);

        // Scroll to the left end if the caret is near the start of the line
        if (EditorTextBox.CaretIndex < currentLineStartIndex + LineStartToleranceInChars)
        {
            EditorScrollViewer.ScrollToLeftEnd();
        }
    }

    private void EditorTextBox_OnRequestBringIntoView(object sender, RequestBringIntoViewEventArgs e)
    {
        // Prevents scroll when the 'EditorTextBox' is first focused
        if (!_editorTextBoxIsFocused)
        {
            e.Handled = true;
        }
    }
}
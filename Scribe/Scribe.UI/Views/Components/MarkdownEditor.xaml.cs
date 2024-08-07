using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using Scribe.Markdown;
using Scribe.Markdown.Nodes;

namespace Scribe.UI.Views.Components;

public partial class MarkdownEditor : UserControl
{
    private const int MarkdownViewMargin = 8;
    
    private const int LineStartToleranceInChars = 5;
    private int _lineCount = 1;
    
    public event EventHandler<string>? EditorTextChanged;
    
    public MarkdownEditor()
    {
        InitializeComponent();

        EditorTextBox.GotFocus += (_, _) => { SetValue(IsTextBoxFocusedPropertyKey, true); };
        EditorTextBox.LostFocus += (_, _) => { SetValue(IsTextBoxFocusedPropertyKey, false); };
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
    
    public bool IsTextBoxFocused => (bool) GetValue( IsTextBoxFocusedPropertyKey.DependencyProperty);
    
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
    
    private static readonly DependencyPropertyKey IsTextBoxFocusedPropertyKey = DependencyProperty.RegisterReadOnly(
        name: nameof(IsTextBoxFocused),
        propertyType: typeof(bool),
        ownerType: typeof(MarkdownEditor),
        typeMetadata: new FrameworkPropertyMetadata()
    );

    public void InsertMarkdownNode<T>() where T : IMarkdownNode
    {
        string markdownText;

        switch (typeof(T))
        {
            default:
                markdownText = "# ";
                break;
        }
        
        var caretIndex = EditorTextBox.CaretIndex;
        EditorTextBox.Text = EditorTextBox.Text.Insert(caretIndex, markdownText);
        EditorTextBox.CaretIndex = caretIndex + markdownText.Length;
    }
    
    private static void OnMarkdownChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var markdownEditor = (MarkdownEditor) d;
        
        if (markdownEditor.InPreviewMode)
        {
            RenderMarkdown(MarkdownParser.Parse(markdownEditor.EditorText), markdownEditor);
        }
    }

    private static void RenderMarkdown(DocumentNode documentRoot, MarkdownEditor markdownEditor)
    {
        markdownEditor.MarkdownViewerPanel.Children.Clear();
        
        foreach (var node in documentRoot.Children)
        {
            var nodeView = GetViewFromMarkdownNode(node, markdownEditor);
            nodeView.Margin = new Thickness(
                left: 0, 
                top: markdownEditor.MarkdownViewerPanel.Children.Count == 0 ? 0 : MarkdownViewMargin,
                right: 0,
                bottom: MarkdownViewMargin
            );
            
            markdownEditor.MarkdownViewerPanel.Children.Add(nodeView);
        }
    }

    private static FrameworkElement GetViewFromMarkdownNode(IMarkdownNode node, MarkdownEditor markdownEditor)
    {
        switch (node)
        {
            case ParagraphNode paragraphNode:
                return new TextBlock
                {
                    Text = paragraphNode.Text,
                    TextWrapping = TextWrapping.Wrap
                };
            case HeaderNode headerNode:
                var headerGrid = new Grid();
                headerGrid.RowDefinitions.Add(new RowDefinition());
                headerGrid.RowDefinitions.Add(new RowDefinition());
                
                if (markdownEditor.Resources[$"Header{headerNode.Level}.Style"] is Style headerStyle)
                {
                    headerGrid.Resources.Add(typeof(TextBlock), headerStyle); 
                }
                
                foreach (var headerChildNode in headerNode.Children)
                {
                    var nodeView = GetViewFromMarkdownNode(headerChildNode, markdownEditor);
                    
                    headerGrid.ColumnDefinitions.Add(new ColumnDefinition());
                    Grid.SetColumn(nodeView, headerGrid.ColumnDefinitions.Count - 1);
                    headerGrid.Children.Add(nodeView);
                }

                var headerDivider = new Rectangle
                {
                    Style = Application.Current.Resources["Style.Divider.Horizontal"] as Style,
                    Margin = new Thickness(0, MarkdownViewMargin, 0, 0)
                };
                Grid.SetRow(headerDivider, 1);
                Grid.SetColumnSpan(headerDivider, headerGrid.ColumnDefinitions.Count);
                headerGrid.Children.Add(headerDivider);
                
                return headerGrid;
            default:
                return new TextBlock { Text = "Markdown Node not implemented" };
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
        if (!IsTextBoxFocused)
        {
            e.Handled = true;
        }
    }
}
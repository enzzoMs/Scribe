using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Shapes;
using Scribe.Markup;
using Scribe.Markup.Inlines;
using Scribe.Markup.Nodes;

namespace Scribe.UI.Views.Components;

public partial class MarkupEditor : UserControl
{
    private const int MarkupViewMargin = 8;
    private const double SubAndSuperscriptFontSizePct = 0.85;

    private const int LineStartToleranceInChars = 5;
    private int _lineCount = 1;
    
    public event EventHandler<string>? EditorTextChanged;
    
    public MarkupEditor()
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
        ownerType: typeof(MarkupEditor),
        typeMetadata: new FrameworkPropertyMetadata(propertyChangedCallback: OnMarkupChanged)
    );

    public static readonly DependencyProperty InPreviewModeProperty = DependencyProperty.Register(
        name: nameof(InPreviewMode),
        propertyType: typeof(bool),
        ownerType: typeof(MarkupEditor),
        typeMetadata: new FrameworkPropertyMetadata(
            defaultValue: false, 
            flags: FrameworkPropertyMetadataOptions.AffectsRender,
            propertyChangedCallback: OnMarkupChanged
        )
    );
    
    private static readonly DependencyPropertyKey IsTextBoxFocusedPropertyKey = DependencyProperty.RegisterReadOnly(
        name: nameof(IsTextBoxFocused),
        propertyType: typeof(bool),
        ownerType: typeof(MarkupEditor),
        typeMetadata: new FrameworkPropertyMetadata()
    );

    public void InsertMarkupNode<T>() where T : IMarkupNode
    {
        string markupText;

        switch (typeof(T))
        {
            default:
                markupText = "= ";
                break;
        }
        
        var caretIndex = EditorTextBox.CaretIndex;
        EditorTextBox.Text = EditorTextBox.Text.Insert(caretIndex, markupText);
        EditorTextBox.CaretIndex = caretIndex + markupText.Length;
    }
    
    private static void OnMarkupChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var markupEditor = (MarkupEditor) d;
        
        if (markupEditor.InPreviewMode)
        {
            RenderMarkup(MarkupParser.ParseText(markupEditor.EditorText), markupEditor);
        }
    }

    private static void RenderMarkup(DocumentNode documentRoot, MarkupEditor markupEditor)
    {
        markupEditor.MarkupViewerPanel.Children.Clear();
        
        foreach (var node in documentRoot.Children)
        {
            var nodeView = GetViewFromMarkupNode(node, markupEditor);
            nodeView.Margin = new Thickness(
                left: 0, 
                top: markupEditor.MarkupViewerPanel.Children.Count == 0 ? 0 : MarkupViewMargin,
                right: 0,
                bottom: MarkupViewMargin
            );
            
            markupEditor.MarkupViewerPanel.Children.Add(nodeView);
        }
    }

    private static FrameworkElement GetViewFromMarkupNode(IMarkupNode node, MarkupEditor markupEditor)
    {
        switch (node)
        {
            case ParagraphNode paragraphNode:
                var paragraphBlock = new TextBlock { TextWrapping = TextWrapping.Wrap };

                foreach (var inline in paragraphNode.Inlines)
                {
                    var inlineRun = new Run(inline.Text);

                    foreach (var modifier in inline.Modifiers)
                    {
                        switch (modifier)
                        {
                            case InlineMarkupModifiers.Bold:
                                inlineRun.FontWeight = FontWeights.Bold;
                                break;
                            case InlineMarkupModifiers.Italic:
                                inlineRun.FontStyle = FontStyles.Italic;
                                break;
                            case InlineMarkupModifiers.Underline:
                                inlineRun.TextDecorations.Add(TextDecorations.Underline);
                                break;
                            case InlineMarkupModifiers.Strikethrough:
                                inlineRun.TextDecorations.Add(TextDecorations.Strikethrough);
                                break;
                            case InlineMarkupModifiers.Superscript:
                                inlineRun.FontSize = SubAndSuperscriptFontSizePct * paragraphBlock.FontSize;
                                inlineRun.BaselineAlignment = BaselineAlignment.Superscript;
                                break;
                            case InlineMarkupModifiers.Subscript:
                                inlineRun.FontSize = SubAndSuperscriptFontSizePct * paragraphBlock.FontSize;
                                inlineRun.BaselineAlignment = BaselineAlignment.Subscript;
                                break;
                        }
                    }
                    
                    paragraphBlock.Inlines.Add(inlineRun);
                }
                return paragraphBlock;
            case HeaderNode headerNode:
                var headerGrid = new Grid();
                headerGrid.RowDefinitions.Add(new RowDefinition());
                headerGrid.RowDefinitions.Add(new RowDefinition());
                
                if (markupEditor.Resources[$"Header{headerNode.Level}.Style"] is Style headerStyle)
                {
                    headerGrid.Resources.Add(typeof(TextBlock), headerStyle); 
                }
                
                foreach (var headerChildNode in headerNode.Children)
                {
                    var nodeView = GetViewFromMarkupNode(headerChildNode, markupEditor);
                    
                    headerGrid.ColumnDefinitions.Add(new ColumnDefinition());
                    Grid.SetColumn(nodeView, headerGrid.ColumnDefinitions.Count - 1);
                    headerGrid.Children.Add(nodeView);
                }

                var headerDivider = new Rectangle
                {
                    Style = Application.Current.Resources["Style.Divider.Horizontal"] as Style,
                    Margin = new Thickness(0, MarkupViewMargin, 0, 0)
                };
                Grid.SetRow(headerDivider, 1);
                Grid.SetColumnSpan(headerDivider, headerGrid.ColumnDefinitions.Count);
                headerGrid.Children.Add(headerDivider);
                
                return headerGrid;
            default:
                return new TextBlock { Text = "Markup Node not implemented" };
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
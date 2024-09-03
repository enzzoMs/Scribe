using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Scribe.Markup;
using Scribe.Markup.Inlines;
using Scribe.Markup.Nodes;
using Scribe.Markup.Nodes.Blocks;
using Scribe.Markup.Nodes.Leafs;

namespace Scribe.UI.Views.Components;

public partial class MarkupEditor : UserControl
{
    private const int TabSize = 4;
    
    private const int MarkupViewMargin = 8;
    private const double SubAndSuperscriptFontSizePct = 0.85;

    private const int LineStartToleranceInChars = 5;
    private int _lineCount = 1;
    
    private static readonly Dictionary<Type, string> MarkupDictionary = new()
    {
        { typeof(HeaderNode), "[#] " },
        { typeof(OrderedListNode), "[1.] " },
        { typeof(UnorderedListNode), "[*] " },
        { typeof(QuoteNode), "[quote] " },
        { typeof(CodeNode), "[code] " }
    };
    
    public event EventHandler<string>? EditorTextChanged;
    
    public MarkupEditor()
    {
        InitializeComponent();

        EditorTextBox.GotFocus += (_, _) => { SetValue(IsTextBoxFocusedPropertyKey, true); };
        EditorTextBox.LostFocus += (_, _) => { SetValue(IsTextBoxFocusedPropertyKey, false); };
        
        DataObject.AddPastingHandler(EditorTextBox, (_, args) =>
        {
            if (args.DataObject.GetData(DataFormats.Text) is not string pastedText) return;
            
            var newDataObject = new DataObject();
            newDataObject.SetData(
                DataFormats.Text, pastedText.Replace("\t", new string(' ', TabSize))
            );
            args.DataObject = newDataObject;
        });
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

    public void InsertMarkupNode(Type markupType)
    {
        if (MarkupDictionary.TryGetValue(markupType, out var markupText))
        {
            var caretIndex = EditorTextBox.CaretIndex;
            EditorTextBox.Text = EditorTextBox.Text.Insert(caretIndex, markupText);
            EditorTextBox.CaretIndex = caretIndex + markupText.Length;
        }
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
        return node switch
        {
            ParagraphNode paragraphNode => GetViewFromMarkupNode(paragraphNode),
            HeaderNode headerNode => GetViewFromMarkupNode(headerNode, markupEditor),
            UnorderedListNode unorderedListNode => GetViewFromMarkupNode(unorderedListNode, markupEditor),
            OrderedListNode orderedListNode => GetViewFromMarkupNode(orderedListNode, markupEditor),
            DividerNode dividerNode => GetViewFromMarkupNode(dividerNode),
            QuoteNode quoteNode => GetViewFromMarkupNode(quoteNode, markupEditor),
            CodeNode codeBlock => GetViewFromMarkupNode(codeBlock, markupEditor),
            _ => new TextBlock { Text = "(Markup Node not implemented)" }
        };
    }

    private static TextBlock GetViewFromMarkupNode(ParagraphNode paragraphNode)
    {
        var paragraphBlock = new TextBlock { TextWrapping = TextWrapping.Wrap };

        foreach (var inline in paragraphNode.Inlines)
        {
            paragraphBlock.Inlines.Add(GetRunFromInline(inline, paragraphBlock.FontSize));
        }

        return paragraphBlock;
    }

    private static Grid GetViewFromMarkupNode(HeaderNode headerNode, MarkupEditor markupEditor)
    {
        var headerGrid = new Grid();
        headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        headerGrid.ColumnDefinitions.Add(new ColumnDefinition());
                
        if (markupEditor.Resources[$"Header{headerNode.Level}.Style"] is Style headerStyle)
        {
            headerGrid.Resources.Add(typeof(TextBlock), headerStyle); 
        }
                
        var headerLevelIndicator = new TextBlock
        {
            Text = new string('#', headerNode.Level),
            Margin = new Thickness(0, 0, right: 8, 0)
        };
        headerGrid.Children.Add(headerLevelIndicator);
                
        foreach (var childNode in headerNode.Children)
        {
            var nodeView = GetViewFromMarkupNode(childNode, markupEditor);
                    
            if (childNode != headerNode.Children.First())
            {
                nodeView.Margin = new Thickness(0, top: 4, 0, 0);
            }
                    
            headerGrid.RowDefinitions.Add(new RowDefinition());
            headerGrid.Children.Add(nodeView);
            Grid.SetRow(nodeView, headerGrid.RowDefinitions.Count - 1);
            Grid.SetColumn(nodeView, 1);
        }
                
        return headerGrid;
    }

    private static Grid GetViewFromMarkupNode(UnorderedListNode unorderedListNode, MarkupEditor markupEditor)
    {
        var unorderedListGrid = new Grid();
        unorderedListGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        unorderedListGrid.ColumnDefinitions.Add(new ColumnDefinition());
                
        var bulletIndicator = new Ellipse
        {
            Fill = Application.Current.Resources["Brush.Text.Primary"] as Brush,
            Width = 6,
            Height = 6,
            Margin = new Thickness(left: 18, top: 7, right: 12, bottom: 0),
            VerticalAlignment = VerticalAlignment.Top
        };
        unorderedListGrid.Children.Add(bulletIndicator); 
        Grid.SetColumn(bulletIndicator, 0);
                
        foreach (var childNode in unorderedListNode.Children)
        {
            var nodeView = GetViewFromMarkupNode(childNode, markupEditor);
            nodeView.VerticalAlignment = VerticalAlignment.Center;

            if (childNode != unorderedListNode.Children.First())
            {
                nodeView.Margin = new Thickness(0, top: 14, 0, 0);
            }

            unorderedListGrid.RowDefinitions.Add(new RowDefinition());
            unorderedListGrid.Children.Add(nodeView); 
            Grid.SetRow(nodeView, unorderedListGrid.RowDefinitions.Count - 1);
            Grid.SetColumn(nodeView, 1);
        }

        return unorderedListGrid;
    }

    private static Grid GetViewFromMarkupNode(OrderedListNode orderedListNode, MarkupEditor markupEditor)
    {
        var orderedListGrid = new Grid();
        orderedListGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        orderedListGrid.ColumnDefinitions.Add(new ColumnDefinition());
                
        var listNumberIndicator = new TextBlock
        {
            Text = $"{orderedListNode.ListNumber}.",
            Margin = new Thickness(left: 18, 0, right: 12, 0),
            VerticalAlignment = VerticalAlignment.Top
        };
        orderedListGrid.Children.Add(listNumberIndicator); 
        Grid.SetColumn(listNumberIndicator, 0);
                
        foreach (var childNode in orderedListNode.Children)
        {
            var nodeView = GetViewFromMarkupNode(childNode, markupEditor);
            nodeView.VerticalAlignment = VerticalAlignment.Center;

            if (childNode != orderedListNode.Children.First())
            {
                nodeView.Margin = new Thickness(0, top: 14, 0, 0);
            }

            orderedListGrid.RowDefinitions.Add(new RowDefinition());
            orderedListGrid.Children.Add(nodeView); 
            Grid.SetRow(nodeView, orderedListGrid.RowDefinitions.Count - 1);
            Grid.SetColumn(nodeView, 1);
        }

        return orderedListGrid;
    }

    private static Grid GetViewFromMarkupNode(DividerNode dividerNode)
    {
        var dividerGrid = new Grid();
        dividerGrid.ColumnDefinitions.Add(new ColumnDefinition {             
            Width = new GridLength(dividerNode.Length, GridUnitType.Star)
        });
        dividerGrid.ColumnDefinitions.Add(new ColumnDefinition
        {
            Width = new GridLength(DividerNode.MaxLength - dividerNode.Length, GridUnitType.Star)
        });
        
        var dividerRectangle = new Rectangle();
        dividerGrid.Children.Add(dividerRectangle);
        
        if (Application.Current.Resources["Style.Divider.Horizontal"] is Style dividerStyle)
        {
            dividerRectangle.Style = dividerStyle;
        }

        return dividerGrid;
    }
    
    private static Grid GetViewFromMarkupNode(QuoteNode quoteNode, MarkupEditor markupEditor)
    {
        var quoteGrid = new Grid();
        quoteGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        quoteGrid.ColumnDefinitions.Add(new ColumnDefinition());
        quoteGrid.Resources.Add(typeof(TextBlock), markupEditor.Resources["QuoteBlock.Style"] as Style);

        var quoteIndicator = new Rectangle
        {
            Width = 3,
            Fill = Application.Current.Resources["Brush.Text.Primary"] as Brush,
            Margin = new Thickness(left: 18, 0, right: 12, 0)
        };
        quoteGrid.Children.Add(quoteIndicator);
        
        foreach (var childNode in quoteNode.Children)
        {
            var nodeView = GetViewFromMarkupNode(childNode, markupEditor);
            
            if (childNode != quoteNode.Children.First())
            {
                nodeView.Margin = new Thickness(0, top: 14, 0, 0);
            }
            
            quoteGrid.RowDefinitions.Add(new RowDefinition());
            quoteGrid.Children.Add(nodeView); 
            Grid.SetRow(nodeView, quoteGrid.RowDefinitions.Count - 1);
            Grid.SetColumn(nodeView, 1);
            
            Grid.SetRowSpan(quoteIndicator, quoteGrid.RowDefinitions.Count);
        }

        return quoteGrid;
    }

    private static Grid GetViewFromMarkupNode(CodeNode codeNode, MarkupEditor markupEditor)
    {
        var codeGrid = new Grid { Style = markupEditor.Resources["CodeBlock.Style"] as Style };
        codeGrid.Resources.Add(typeof(TextBlock), markupEditor.Resources["CodeBlock.Text.Style"]);
        
        foreach (var childNode in codeNode.Children)
        {
            var nodeView = GetViewFromMarkupNode(childNode, markupEditor);
            
            nodeView.Margin = new Thickness(left: 14, top: 14, right: 14, bottom: 0);
            
            codeGrid.RowDefinitions.Add(new RowDefinition());
            codeGrid.Children.Add(nodeView); 
            Grid.SetRow(nodeView, codeGrid.RowDefinitions.Count - 1);
        }

        return codeGrid;
    }
    
    private static Run GetRunFromInline(InlineMarkup inline, double paragraphFontSize)
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
                    inlineRun.FontSize = SubAndSuperscriptFontSizePct * paragraphFontSize;
                    inlineRun.BaselineAlignment = BaselineAlignment.Superscript;
                    break;
                case InlineMarkupModifiers.Subscript:
                    inlineRun.FontSize = SubAndSuperscriptFontSizePct * paragraphFontSize;
                    inlineRun.BaselineAlignment = BaselineAlignment.Subscript;
                    break;
            }
        }

        return inlineRun;
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

    private void EditorTextBox_OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Tab)
        {
            EditorTextBox.SelectedText = "";

            var absoluteCaretIndex = EditorTextBox.CaretIndex;
            var caretIndexInCurrentLine = EditorTextBox.CaretIndex - EditorTextBox.GetCharacterIndexFromLineIndex(
                EditorTextBox.GetLineIndexFromCharacterIndex(EditorTextBox.CaretIndex)
            );
            var tabSize = TabSize - (caretIndexInCurrentLine % TabSize);
            var tab = new string(' ', tabSize);

            EditorTextBox.BeginChange();
            
            var editorText = EditorTextBox.Text;
            EditorTextBox.Clear();
            EditorTextBox.AppendText(editorText.Insert(absoluteCaretIndex, tab));
            EditorTextBox.CaretIndex = absoluteCaretIndex + tabSize;
            
            EditorTextBox.EndChange();

            e.Handled = true;
        }
    }
}
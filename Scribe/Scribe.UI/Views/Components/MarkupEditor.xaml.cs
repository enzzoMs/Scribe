using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using Scribe.Markup;
using Scribe.Markup.Inlines;
using Scribe.Markup.Nodes;
using Scribe.Markup.Nodes.Blocks;
using Scribe.Markup.Nodes.Leafs;

namespace Scribe.UI.Views.Components;

public partial class MarkupEditor : UserControl
{
    private const int MarkupViewMargin = 8;
    private const double SubAndSuperscriptFontSizePct = 0.85;
    
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
        
        EditorTextBox.TextArea.TextView.Margin = new Thickness(12, 0, 0, 0);
        
        EditorTextBox.GotFocus += (_, _) => { SetValue(IsTextBoxFocusedPropertyKey, true); };
        EditorTextBox.LostFocus += (_, _) => { SetValue(IsTextBoxFocusedPropertyKey, false); };
        
        var markupColor = new XshdColor
        {
            Foreground = new SimpleHighlightingBrush(Colors.Black),
            FontWeight = FontWeights.Bold
        };
        var markupColorReference = new XshdReference<XshdColor>(markupColor);
        
        var markupBlockBeginRule = new XshdRule
        {
            // Pattern for block markup. E.g. [quote], [quote]% 
            Regex = @"(?<=^|(^[\t ]*))\[[^\]]+\]%?",
            ColorReference = markupColorReference,
        };
        
        var markupBlockEndRule = new XshdRule
        {
            // Pattern for end of block ( % )
            Regex = @"(?<=^)[\t ]*%[\t\r ]*(?=$)",
            ColorReference = markupColorReference
        };

        var markupInlineBeginRule = new XshdRule
        {
            Regex = @"{(?=[^}\n]+?}(\[[^\]\n]*?\]))",
            ColorReference = markupColorReference
        };
        
        var markupInlineEndRule = new XshdRule {
            Regex = @"(?<={[^}\n]+?)}\[[^\]\n]*?\]",
            ColorReference = markupColorReference
        };
        
        // Pattern for dividers. E.g. "-----" 
        var dividerRule = new XshdRule { Regex = @"^[\t ]*-+[\t\r ]*(?=$)", ColorReference = markupColorReference };
        
        var newLineRule = new XshdRule { Regex = @"\/\/", ColorReference = markupColorReference };
        
        var mainRuleSet = new XshdRuleSet();
        mainRuleSet.Elements.Add(markupBlockBeginRule);
        mainRuleSet.Elements.Add(markupBlockEndRule);
        mainRuleSet.Elements.Add(markupInlineBeginRule);
        mainRuleSet.Elements.Add(markupInlineEndRule);
        mainRuleSet.Elements.Add(dividerRule);
        mainRuleSet.Elements.Add(newLineRule);

        var syntaxDefinition = new XshdSyntaxDefinition();
        syntaxDefinition.Elements.Add(mainRuleSet);
        
        EditorTextBox.SyntaxHighlighting = HighlightingLoader.Load(syntaxDefinition, HighlightingManager.Instance);
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
    
    public bool IsTextBoxFocused => (bool) GetValue(IsTextBoxFocusedPropertyKey.DependencyProperty);
    
    public static readonly DependencyProperty EditorTextProperty = DependencyProperty.Register(
        name: nameof(EditorText),
        propertyType: typeof(string),
        ownerType: typeof(MarkupEditor),
        typeMetadata: new FrameworkPropertyMetadata(propertyChangedCallback: OnEditorTextChanged)
    );

    public static readonly DependencyProperty InPreviewModeProperty = DependencyProperty.Register(
        name: nameof(InPreviewMode),
        propertyType: typeof(bool),
        ownerType: typeof(MarkupEditor),
        typeMetadata: new FrameworkPropertyMetadata(
            defaultValue: false, 
            flags: FrameworkPropertyMetadataOptions.AffectsRender,
            propertyChangedCallback: OnPreviewModeChanged
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
            EditorTextBox.TextArea.Document.Insert(EditorTextBox.CaretOffset, markupText);
        }
    }
    
    private static void OnPreviewModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var markupEditor = (MarkupEditor) d;
        
        if (markupEditor.InPreviewMode)
        {
            RenderMarkup(MarkupParser.ParseText(markupEditor.EditorTextBox.Text), markupEditor);
        }
    }
    
    private static void OnEditorTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var markupEditor = (MarkupEditor) d;

        var undoStackLimit = markupEditor.EditorTextBox.Document.UndoStack.SizeLimit;
        
        // Disabling the undo stack
        markupEditor.EditorTextBox.Document.UndoStack.SizeLimit = 0;
        
        markupEditor.EditorTextBox.Text = markupEditor.EditorText;
        markupEditor.EditorTextBox.Document.UndoStack.SizeLimit = undoStackLimit;

        if (markupEditor.InPreviewMode)
        {
            RenderMarkup(MarkupParser.ParseText(markupEditor.EditorText), markupEditor);
        }
    }

    private static void RenderMarkup(DocumentNode documentRoot, MarkupEditor markupEditor)
    {
        markupEditor.MarkupViewerPanel.Items.Clear();
        
        foreach (var node in documentRoot.Children)
        {
            var nodeView = GetViewFromMarkupNode(node, markupEditor);

            nodeView.Margin = new Thickness(
                left: 0,
                top: markupEditor.MarkupViewerPanel.Items.Count == 0 ? 0 : MarkupViewMargin,
                right: 0,
                bottom: MarkupViewMargin
            );

            markupEditor.MarkupViewerPanel.Items.Add(nodeView);
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

        var childrenList = new ItemsControl();
        headerGrid.Children.Add(childrenList);
        Grid.SetColumn(childrenList, 1);
                
        foreach (var childNode in headerNode.Children)
        {
            var nodeView = GetViewFromMarkupNode(childNode, markupEditor);
                    
            if (childNode != headerNode.Children.First())
            {
                nodeView.Margin = new Thickness(0, top: 4, 0, 0);
            }
                    
            childrenList.Items.Add(nodeView);
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
                
        var childrenList = new ItemsControl();
        unorderedListGrid.Children.Add(childrenList);
        Grid.SetColumn(childrenList, 1);
        
        foreach (var childNode in unorderedListNode.Children)
        {
            var nodeView = GetViewFromMarkupNode(childNode, markupEditor);
            nodeView.VerticalAlignment = VerticalAlignment.Center;

            if (childNode != unorderedListNode.Children.First())
            {
                nodeView.Margin = new Thickness(0, top: 14, 0, 0);
            }

            childrenList.Items.Add(nodeView);
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

        var childrenList = new ItemsControl();
        orderedListGrid.Children.Add(childrenList);
        Grid.SetColumn(childrenList, 1);
        
        foreach (var childNode in orderedListNode.Children)
        {
            var nodeView = GetViewFromMarkupNode(childNode, markupEditor);
            nodeView.VerticalAlignment = VerticalAlignment.Center;

            if (childNode != orderedListNode.Children.First())
            {
                nodeView.Margin = new Thickness(0, top: 14, 0, 0);
            }

            childrenList.Items.Add(nodeView); 
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
        
        var childrenList = new ItemsControl();
        quoteGrid.Children.Add(childrenList);
        Grid.SetColumn(childrenList, 1);
        
        foreach (var childNode in quoteNode.Children)
        {
            var nodeView = GetViewFromMarkupNode(childNode, markupEditor);
            
            if (childNode != quoteNode.Children.First())
            {
                nodeView.Margin = new Thickness(0, top: 14, 0, 0);
            }
            
            childrenList.Items.Add(nodeView);
        }

        return quoteGrid;
    }

    private static Border GetViewFromMarkupNode(CodeNode codeNode, MarkupEditor markupEditor)
    {
        var codeList = new ItemsControl();
        codeList.Resources.Add(typeof(TextBlock), markupEditor.Resources["CodeBlock.Text.Style"]);

        var codeBorder = new Border
        {
            Style = markupEditor.Resources["CodeBlock.Border.Style"] as Style,
            Child = codeList
        };

        foreach (var childNode in codeNode.Children)
        {
            var nodeView = GetViewFromMarkupNode(childNode, markupEditor);
            codeList.Items.Add(nodeView); 
        }

        return codeBorder;
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
    
    private void EditorTextBox_OnTextChanged(object? sender, EventArgs e)
    {
        EditorTextChanged?.Invoke(this, EditorTextBox.Text);
    }
}
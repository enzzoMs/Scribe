using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
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
    private const int MarkupViewMargin = 14;
    private const double SubAndSuperscriptFontSizePct = 0.85;
    
    private static readonly Dictionary<Type, string> MarkupDictionary = new()
    {
        { typeof(HeaderNode), "[#] " },
        { typeof(OrderedListNode), "[1.] " },
        { typeof(UnorderedListNode), "[*] " },
        { typeof(QuoteNode), "[quote] " },
        { typeof(CodeNode), "[code] " },
        { typeof(TaskListNode), "[-] " }
    };
    
    public event EventHandler<string>? EditorTextChanged;
    
    public MarkupEditor()
    {
        InitializeComponent();
        
        EditorTextBox.TextArea.TextView.Margin = new Thickness(12, 0, 0, 0);
        
        EditorTextBox.GotFocus += (_, _) => { SetValue(IsTextBoxFocusedPropertyKey, true); };
        EditorTextBox.LostFocus += (_, _) => { SetValue(IsTextBoxFocusedPropertyKey, false); };
        
        EditorTextBox.TextArea.TextView.LinkTextForegroundBrush = Brushes.Black;
        EditorTextBox.TextArea.TextView.LinkTextUnderline = false;
        
        var markupColor = new XshdColor { Foreground = new SimpleHighlightingBrush(Colors.Black), FontWeight = FontWeights.Bold };
        var markupColorReference = new XshdReference<XshdColor>(markupColor);
        
        // Pattern for block markup. E.g. [quote], [quote]% 
        var markupBlockBeginRule = new XshdRule { Regex = @"(?<=^|(^[\t ]*))\[[^\]]*[^\\]\]%?", ColorReference = markupColorReference };
        
        // Pattern for end of block ( % )
        var markupBlockEndRule = new XshdRule { Regex = @"(?<=^)[\t ]*%[\t\r ]*(?=$)", ColorReference = markupColorReference };

        // Pattern for inline markup. E.g. {text}[b,i]
        var markupInlineBeginRule = new XshdRule { Regex = @"{(?=[^}\n]+?}(\[\]|\[[^\]\n]*?[^\\]\]))", ColorReference = markupColorReference };
        var markupInlineEndRule = new XshdRule { Regex = @"(?<={[^}\n]+?)}(\[\]|\[[^\]\n]*?[^\\]\])", ColorReference = markupColorReference };
        
        // Pattern for dividers. E.g. "-----" 
        var dividerRule = new XshdRule { Regex = @"^[\t ]*-+[\t\r ]*(?=$)", ColorReference = markupColorReference };

        var keywords = new XshdKeywords();
        keywords.Words.Add("///");
        keywords.Words.Add(@"\/");
        keywords.Words.Add(@"\[");
        keywords.Words.Add(@"\]");
        keywords.Words.Add(@"\{");
        keywords.Words.Add(@"\}");
        keywords.Words.Add(@"\%");
        keywords.Words.Add(@"\\");
        keywords.ColorReference = markupColorReference;
        
        var mainRuleSet = new XshdRuleSet();
        mainRuleSet.Elements.Add(markupBlockBeginRule);
        mainRuleSet.Elements.Add(markupBlockEndRule);
        mainRuleSet.Elements.Add(markupInlineBeginRule);
        mainRuleSet.Elements.Add(markupInlineEndRule);
        mainRuleSet.Elements.Add(dividerRule);
        mainRuleSet.Elements.Add(keywords);

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
                bottom: 0
            );

            markupEditor.MarkupViewerPanel.Items.Add(nodeView);
        }
    }

    private static FrameworkElement GetViewFromMarkupNode(IMarkupNode node, MarkupEditor markupEditor)
    {
        return node switch
        {
            ParagraphNode paragraphNode => GetParagraphView(paragraphNode),
            DividerNode dividerNode => GetDividerView(dividerNode),
            IBlockNode blockNode => GetViewFromBlockNode(blockNode, markupEditor),
            _ => new TextBlock { Text = "(Markup Node not implemented)" }
        };
    }

    private static TextBlock GetParagraphView(ParagraphNode paragraphNode)
    {
        var paragraphBlock = new TextBlock { TextWrapping = TextWrapping.Wrap };

        foreach (var inline in paragraphNode.Inlines)
        {
            paragraphBlock.Inlines.Add(GetRunFromInline(inline, paragraphBlock.FontSize));
        }

        return paragraphBlock;
    }

    private static Grid GetDividerView(DividerNode dividerNode)
    {
        var dividerGrid = new Grid();
        dividerGrid.ColumnDefinitions.Add(new ColumnDefinition {             
            Width = new GridLength(dividerNode.Length, GridUnitType.Star)
        });
        dividerGrid.ColumnDefinitions.Add(new ColumnDefinition
        {
            Width = new GridLength(DividerNode.MaxLength - dividerNode.Length, GridUnitType.Star)
        });
        
        var dividerRectangle = new Rectangle { Style = Application.Current.Resources["Style.Divider.Horizontal"] as Style };
        dividerGrid.Children.Add(dividerRectangle);
        
        return dividerGrid;
    }

    private static FrameworkElement GetViewFromBlockNode(IBlockNode blockNode, MarkupEditor markupEditor)
    {
        var blockGrid = new Grid();
        blockGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        blockGrid.ColumnDefinitions.Add(new ColumnDefinition());
        
        switch (blockNode)
        {   
            case HeaderNode headerNode:
                if (markupEditor.Resources[$"Header{headerNode.Level}.Style"] is Style headerStyle)
                {
                    blockGrid.Resources.Add(typeof(TextBlock), headerStyle); 
                }
                blockGrid.Children.Add(new TextBlock
                {
                    Text = new string('#', headerNode.Level), 
                    Margin = new Thickness(0, 0, right: 8, 0)
                });
                break;
            case UnorderedListNode:
                blockGrid.Children.Add(new Ellipse { Style = markupEditor.Resources["BulletIndicator.Style"] as Style });
                break;
            case OrderedListNode orderedListNode:
                blockGrid.Children.Add(new TextBlock
                {
                    Text = $"{orderedListNode.ListNumber}.", 
                    Style = markupEditor.Resources["ListIndicator.Style"] as Style
                });
                break;
            case QuoteNode:
                blockGrid.Resources.Add(typeof(TextBlock), markupEditor.Resources["QuoteBlock.Text.Style"] as Style);
                blockGrid.Children.Add(new Rectangle { Style = markupEditor.Resources["QuoteBlock.Indicator.Style"] as Style });
                break;
            case TaskListNode taskListNode:
                if (taskListNode.IsChecked)
                {
                    blockGrid.Resources.Add(typeof(TextBlock), markupEditor.Resources["CheckBox.Checked.Text.Style"]);
                }
                
                blockGrid.Children.Add(new CheckBox { IsChecked = taskListNode.IsChecked });
                break;
        }
        
        var childrenList = new ItemsControl();
        blockGrid.Children.Add(childrenList);
        Grid.SetColumn(childrenList, 1);
        
        foreach (var childNode in blockNode.Children)
        {
            var nodeView = GetViewFromMarkupNode(childNode, markupEditor);
            nodeView.VerticalAlignment = VerticalAlignment.Center;

            if (childNode != blockNode.Children.First())
            {
                nodeView.Margin = new Thickness(0, top: MarkupViewMargin, 0, 0);
            }

            childrenList.Items.Add(nodeView); 
        }

        if (blockNode is QuoteNode { Author: not null } quoteNode)
        {
            var authorBlock = new TextBlock();
            authorBlock.Inlines.Add(new Run($"\n- {quoteNode.Author}") { FontWeight = FontWeights.Bold });
            childrenList.Items.Add(authorBlock);
        } 
        
        if (blockNode is CodeNode)
        {
            blockGrid.Resources.Add(typeof(TextBlock), markupEditor.Resources["CodeBlock.Text.Style"]);
            return new Border { Child = blockGrid, Style = markupEditor.Resources["CodeBlock.Border.Style"] as Style };     
        }
        
        return blockGrid;
    }
    
    private static Run GetRunFromInline(InlineMarkup inlineMarkup, double paragraphFontSize)
    {
        var inline = new Run(inlineMarkup.Text);

        if (inlineMarkup.Uri != null)
        {
            inline.TextDecorations.Add(TextDecorations.Underline);

            Uri.TryCreate(inlineMarkup.Uri, UriKind.Absolute, out var uri);
            
            if (uri != null && uri.Scheme is "http" or "https" or "file")
            {
                inline.Cursor = Cursors.Hand;
                inline.Foreground = Brushes.Blue;
                inline.PreviewMouseLeftButtonDown += (_, _) =>
                {
                    try
                    {
                        Process.Start(new ProcessStartInfo { FileName = inlineMarkup.Uri, UseShellExecute = true });
                    }
                    catch
                    {
                        var uriErrorMessage = string.Format(
                            Application.Current.Resources["String.Error.OpenLink"] as string ?? "", inlineMarkup.Uri
                        );
                        
                        new MessageBox
                        {
                            Owner = Application.Current.MainWindow,
                            Title = Application.Current.Resources["String.Error"] as string,
                            MessageIconPath = Application.Current.Resources["Drawing.Exclamation"] as Geometry,
                            Message = uriErrorMessage
                        }.ShowDialog();
                    }
                };
            }
            else
            {
                inline.Foreground = Brushes.Gray;
            }    
        }
        
        if (inlineMarkup.Foreground != null)
        {
            inline.Foreground = new SolidColorBrush(new Color
            {
                A = inlineMarkup.Foreground.Value.A, R = inlineMarkup.Foreground.Value.R, 
                G = inlineMarkup.Foreground.Value.G, B = inlineMarkup.Foreground.Value.B
            });
        }
        
        if (inlineMarkup.Background != null)
        {
            inline.Background = new SolidColorBrush(new Color
            {
                A = inlineMarkup.Background.Value.A, R = inlineMarkup.Background.Value.R, 
                G = inlineMarkup.Background.Value.G, B = inlineMarkup.Background.Value.B
            });
        }
        
        var previousBackground = inline.Background;
        var previousForeground = inline.Foreground;
        
        foreach (var modifier in inlineMarkup.Modifiers)
        {
            switch (modifier)
            {
                case InlineMarkupModifiers.Bold:
                    inline.FontWeight = FontWeights.Bold;
                    break;
                case InlineMarkupModifiers.Italic:
                    inline.FontStyle = FontStyles.Italic;
                    break;
                case InlineMarkupModifiers.Underline:
                    inline.TextDecorations.Add(TextDecorations.Underline);
                    break;
                case InlineMarkupModifiers.Strikethrough:
                    inline.TextDecorations.Add(TextDecorations.Strikethrough);
                    break;
                case InlineMarkupModifiers.Superscript:
                    inline.FontSize = SubAndSuperscriptFontSizePct * paragraphFontSize;
                    inline.BaselineAlignment = BaselineAlignment.Superscript;
                    break;
                case InlineMarkupModifiers.Subscript:
                    inline.FontSize = SubAndSuperscriptFontSizePct * paragraphFontSize;
                    inline.BaselineAlignment = BaselineAlignment.Subscript;
                    break;
                case InlineMarkupModifiers.Code:
                    inline.FontFamily = Application.Current.Resources["Text.Monospace"] as FontFamily;
                    inline.Background = Application.Current.Resources["Brush.Markup.Code"] as SolidColorBrush;
                    break;
                case InlineMarkupModifiers.Spoiler:
                    inline.Background = Application.Current.Resources["Brush.Markup.Code"] as SolidColorBrush;
                    inline.Foreground = Application.Current.Resources["Brush.Markup.Code"] as SolidColorBrush;
                    inline.Cursor = Cursors.Hand;
                    inline.PreviewMouseLeftButtonDown += (_, _) =>
                    {
                        inline.Foreground = previousForeground;
                        inline.Background = previousBackground;
                        inline.Cursor = Cursors.Arrow;
                    };
                    break;
            }
        }

        return inline;
    }
    
    private void EditorTextBox_OnTextChanged(object? sender, EventArgs e)
    {
        EditorTextChanged?.Invoke(this, EditorTextBox.Text);
    }
}
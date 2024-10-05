using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using ICSharpCode.AvalonEdit.Search;
using Scribe.Markup;
using Scribe.Markup.Inlines;
using Scribe.Markup.Nodes;
using Scribe.Markup.Nodes.Blocks;
using Scribe.Markup.Nodes.Leafs;
using Path = System.Windows.Shapes.Path;

namespace Scribe.UI.Views.Components;

public partial class MarkupEditor : UserControl
{
    private const double SubAndSuperscriptFontSizePct = 0.85;
    public const int MarkupViewMargin = 18;

    private static readonly Dictionary<Type, string> MarkupBlockDictionary = new()
    {
        { typeof(HeaderNode), "[#] " },
        { typeof(OrderedListNode), "[1.] " },
        { typeof(UnorderedListNode), "[*] " },
        { typeof(QuoteNode), "[quote] " },
        { typeof(CodeNode), "[code] " },
        { typeof(TaskListNode), "[-] " },
        { typeof(ImageNode), "[img(100%)= ]" },
        { typeof(ToggleListNode), "[toggle]" },
        { typeof(CalloutNode), "[::callout]" },
        { typeof(ProgressBarNode), "(ooo..)" },
        { typeof(IndentedNode), "[>>]" },
        { typeof(LabelNode), "@label=A" },
        { typeof(TableNode), "[table]%\n\t[cell] Table\n\t=====\n\t[cell] \n%"}
    };
    
    public event EventHandler<string>? EditorTextChanged;
    
    public MarkupEditor()
    {
        InitializeComponent();
        ConfigureEditorTextBox();        
        ConfigureSyntaxHighlighting();
    }
    
    public IEnumerable<IMarkupNode>? MarkupNodes { get; private set; }
    
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
    
    public ICommand? OpenDocumentByNameCommand
    {
        get => (ICommand?) GetValue(OpenDocumentByNameCommandProperty);
        set => SetValue(OpenDocumentByNameCommandProperty, value);
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
    
    public static readonly DependencyProperty OpenDocumentByNameCommandProperty = DependencyProperty.Register(
        name: nameof(OpenDocumentByNameCommand),
        propertyType: typeof(ICommand),
        ownerType: typeof(MarkupEditor)
    );
    
    /// <returns>The image bytes</returns>
    public byte[] GetMarkupAsImage()
    {
        if (MarkupViewerPanel.Items[0] != null)
        {
            MarkupViewerPanel.ScrollIntoView(MarkupViewerPanel.Items[0]!);
        }
        MarkupViewerPanel.UpdateLayout();

        const int rightMargin = 40;
        const int bottomMargin = 50;
        
        var panelWidth = (int) MarkupViewerPanel.ActualWidth + rightMargin;
        var panelHeight = (int) MarkupViewerPanel.ActualHeight + bottomMargin;

        var renderBitmap = new RenderTargetBitmap(panelWidth, panelHeight, 96d, 96d, PixelFormats.Pbgra32);
        renderBitmap.Render(MarkupViewerPanel);

        using var outStream = new MemoryStream();
        
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(renderBitmap));
        encoder.Save(outStream);

        return outStream.ToArray();
    }
    
    public void InsertBlockNode(Type markupType)
    {
        if (MarkupBlockDictionary.TryGetValue(markupType, out var markupText))
        {
            EditorTextBox.TextArea.Document.Insert(EditorTextBox.CaretOffset, markupText);
        }
    }

    public void InsertInlineModifier(InlineMarkupModifiers modifier)
    {
        var modifierText = modifier switch
        {
            InlineMarkupModifiers.Bold => "b",
            InlineMarkupModifiers.Italic => "i",
            InlineMarkupModifiers.Underline => "u",
            InlineMarkupModifiers.Strikethrough => "s",
            InlineMarkupModifiers.Superscript => "super",
            InlineMarkupModifiers.Subscript => "sub",
            InlineMarkupModifiers.Code => "code",
            InlineMarkupModifiers.Spoiler => "spoiler",
            _ => ""
        };
        InsertMarkupModifier(modifierText);
    }
    
    public void InsertColorModifier() => InsertMarkupModifier("foreg=#blue");

    public void InsertLinkModifier() => InsertMarkupModifier("link=");

    public void RenderTextAsMarkup(string text)
    {
        MarkupNodes = MarkupParser.ParseText(text);
        MarkupViewerPanel.ItemsSource = MarkupNodes;   
    }
    
    private void ConfigureEditorTextBox()
    {
        EditorTextBox.TextArea.TextView.Margin = new Thickness(12, 0, 0, 0);
        
        EditorTextBox.GotFocus += (_, _) => { SetValue(IsTextBoxFocusedPropertyKey, true); };
        EditorTextBox.LostFocus += (_, _) => { SetValue(IsTextBoxFocusedPropertyKey, false); };
        
        EditorTextBox.TextArea.TextView.LinkTextForegroundBrush = Brushes.Black;
        EditorTextBox.TextArea.TextView.LinkTextUnderline = false;
        SearchPanel.Install(EditorTextBox);
    }

    private void ConfigureSyntaxHighlighting()
    {
        var markupColor = new XshdColor { Foreground = new SimpleHighlightingBrush(Colors.Black), FontWeight = FontWeights.Bold };
        var markupColorReference = new XshdReference<XshdColor>(markupColor);
        
        var mainRuleSet = new XshdRuleSet
        {
            Elements =
            {
                // Pattern for block markup. E.g. [quote], [quote]% 
                new XshdRule { Regex = @"(?<=^|(^[\t ]*))\[[^\]]+\]%?", ColorReference = markupColorReference },
                // Pattern for end of block ( % )
                new XshdRule { Regex = @"(?<=^)[\t ]*%[\t\r ]*(?=$)", ColorReference = markupColorReference },
                // Pattern for labels ( @label=example )
                new XshdRule { Regex = @"^\s*@label=.+$", ColorReference = markupColorReference },
                // Pattern for progress bars ( (ooo...) )
                new XshdRule { Regex = @"^[\t ]*\(((o+\.*)|(o*\.+))\)[\t ]*$", ColorReference = markupColorReference },
                // Pattern for inline markup. E.g. {text}[b,i]
                new XshdRule { Regex = @"(?<!\$){(?=[^}\n]+?}\[[^\]\n]*?\])", ColorReference = markupColorReference },
                new XshdRule { Regex = @"(?<=(?<!\$){[^}\n]+?)}\[[^\]\n]*?\]", ColorReference = markupColorReference },
                // Pattern for new lines ( '///' )
                new XshdRule { Regex = @"(?<!\$)\/\/\/", ColorReference = markupColorReference },
                // Pattern for the ignore command ( '$' )
                new XshdRule { Regex = @"\$+", ColorReference = markupColorReference }
            }
        };
        
        var syntaxDefinition = new XshdSyntaxDefinition();
        syntaxDefinition.Elements.Add(mainRuleSet);
        
        EditorTextBox.SyntaxHighlighting = HighlightingLoader.Load(syntaxDefinition, HighlightingManager.Instance);
    }
    
    private void InsertMarkupModifier(string modifier)
    {
        if (EditorTextBox.TextArea.Selection.IsEmpty)
        {
            EditorTextBox.TextArea.Document.Insert(EditorTextBox.CaretOffset, $"{{}}[{modifier}]");
            return;
        }
        var textSelection = EditorTextBox.TextArea.Selection;
        textSelection.ReplaceSelectionWithText($"{{{textSelection.GetText()}}}[{modifier}]");
    }
    
    private void EditorTextBox_OnTextChanged(object? sender, EventArgs e)
    {
        EditorTextChanged?.Invoke(this, EditorTextBox.Text);
    }
    
    private static void OnPreviewModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var markupEditor = (MarkupEditor) d;
        
        if (markupEditor.InPreviewMode)
        {
            markupEditor.RenderTextAsMarkup(markupEditor.EditorTextBox.Text);
        }

        markupEditor.MarkupViewerPanel.Visibility = markupEditor.InPreviewMode ? Visibility.Visible : Visibility.Hidden;
        markupEditor.EditorTextBox.Visibility = markupEditor.InPreviewMode ? Visibility.Hidden : Visibility.Visible;
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
            markupEditor.RenderTextAsMarkup(markupEditor.EditorText);
        }
    }

    public FrameworkElement GetViewFromMarkupNode(IMarkupNode node)
    {
        return node switch
        {
            ParagraphNode paragraphNode => GetParagraphView(paragraphNode),
            DividerNode dividerNode => GetDividerView(dividerNode),
            LabelNode labelNode => GetLabelView(labelNode),
            ProgressBarNode progressBarNode => GetProgressBarView(progressBarNode),
            TableNode tableNode => GetTableView(tableNode),
            IBlockNode blockNode => GetViewFromBlockNode(blockNode),
            _ => new TextBlock { Text = "(Markup Node not implemented)" }
        };
    }

    private TextBlock GetParagraphView(ParagraphNode paragraphNode)
    {
        var paragraphBlock = new TextBlock { TextWrapping = TextWrapping.Wrap };

        foreach (var inline in paragraphNode.Inlines)
        {
            paragraphBlock.Inlines.Add(GetRunFromInline(inline, paragraphBlock.FontSize));
        }

        return paragraphBlock;
    }

    private Grid GetDividerView(DividerNode dividerNode)
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
    
    private StackPanel GetProgressBarView(ProgressBarNode progressBarNode)
    {
        var progressBarPanel = new StackPanel { Orientation = Orientation.Horizontal };
        for (var i = 0; i < progressBarNode.MaxLength; i++)
        {
            progressBarPanel.Children.Add(new Border
            {
                Style = Resources["ProgressBar.Item.Style"] as Style,
                Background = i < progressBarNode.Length ? Brushes.Black : null
            });
        }

        progressBarPanel.Children.Add(new TextBlock
        {
            Text = $"{((double)progressBarNode.Length / progressBarNode.MaxLength) * 100:0}%",
            Margin = new Thickness(8, 0, 0, 3)
        });
        
        return progressBarPanel;
    }

    private FrameworkElement GetLabelView(LabelNode labelNode) => new() { Tag = labelNode.Name };

    private ScrollViewer GetTableView(TableNode tableNode)
    {
        var tableGrid = new Grid();
        var tableView = new ScrollViewer
        {
            Content = tableGrid,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            VerticalScrollBarVisibility = ScrollBarVisibility.Disabled,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        tableView.PreviewMouseWheel += OnTableViewScrolled;
        
        var tableCells = tableNode.Children.OfType<TableCellNode>().ToList();
        if (tableCells.Count == 0) return tableView;
        
        var numOfRows = tableCells.Select(c => c.RowNumber).Max() + 1;
        var numOfColumns = tableCells.Select(c => c.ColumnNumber).Max() + 1;
        
        for (var i = numOfRows; i > 0; i--)
        {
            tableGrid.RowDefinitions.Add(new RowDefinition());
        }
        for (var i = numOfColumns; i > 0; i--)
        {
            tableGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        }

        var filledCells = new HashSet<(int Row, int Column)>();
        
        foreach (var cell in tableCells)
        {
            var cellView = GetTableCellView(
                numOfRows, numOfColumns, cell.RowNumber, cell.ColumnNumber, cell
            );
            Grid.SetRow(cellView, cell.RowNumber);
            Grid.SetColumn(cellView, cell.ColumnNumber);
            tableGrid.Children.Add(cellView);

            filledCells.Add((cell.RowNumber, cell.ColumnNumber));
        }

        for (var row = 0; row < numOfRows; row++)
        {
            for (var column = 0; column < numOfColumns; column++)
            {
                if (filledCells.Contains((row, column))) continue;
                
                var emptyCellView = GetTableCellView(numOfRows, numOfColumns, row, column);
                Grid.SetRow(emptyCellView, row);
                Grid.SetColumn(emptyCellView, column);
                tableGrid.Children.Add(emptyCellView);
            }
        }
        
        return tableView;
        
        static void OnTableViewScrolled(object sender, MouseWheelEventArgs e)
        {
            var tableScrollViewer = (ScrollViewer) sender;

            FrameworkElement? currentView = tableScrollViewer;
            
            while (currentView != null)
            {
                currentView = VisualTreeHelper.GetParent(currentView) as FrameworkElement;

                if (currentView is not ScrollViewer markupViewerScrollPanel) continue;
                
                markupViewerScrollPanel.ScrollToVerticalOffset(markupViewerScrollPanel.VerticalOffset - e.Delta);
                e.Handled = true;
                return;
            }
        }
    }

    private Border GetTableCellView(
        int numOfRows, int numOfColumns, int cellRow, int cellColumn, TableCellNode? cell = null
    )
    {
        var cellView = cell == null ? null : GetViewFromBlockNode(cell);
        var cellBorder = new Border
        {
            Style = Resources["TableCell.Style"] as Style,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Child = cellView
        };

        if (cellColumn == numOfColumns - 1)
        {
            cellBorder.BorderThickness = cellBorder.BorderThickness with { Right = 1.3 };
        }

        if (cellRow == numOfRows - 1)
        {
            cellBorder.BorderThickness = cellBorder.BorderThickness with { Bottom = 1.3 };
        }

        if (cellRow == 0)
        {
            if (cellView != null)
            {
                cellView.HorizontalAlignment = HorizontalAlignment.Center;
            }
            cellBorder.Background = Application.Current.Resources["Brush.Markup.Surface"] as SolidColorBrush;
        }
        
        return cellBorder;
    }
    
    private FrameworkElement GetViewFromBlockNode(IBlockNode blockNode)
    {
        var blockGrid = new Grid();
        blockGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        blockGrid.ColumnDefinitions.Add(new ColumnDefinition());
        
        var childrenList = new ItemsControl();
        blockGrid.Children.Add(childrenList);
        Grid.SetColumn(childrenList, 1);

        foreach (var childNode in blockNode.Children)
        {
            var nodeView = GetViewFromMarkupNode(childNode);
            nodeView.VerticalAlignment = VerticalAlignment.Center;

            if (childNode != blockNode.Children.First())
            {
                nodeView.Margin = new Thickness(0, top: MarkupViewMargin, 0, 0);
            }

            childrenList.Items.Add(nodeView); 
        }
        
        AddSpecialBlockElements(blockNode, blockGrid, childrenList);

        switch (blockNode)
        {
            case CodeNode:
                blockGrid.Resources.Add(typeof(TextBlock), Resources["CodeBlock.Text.Style"]);
                return new Border { Child = blockGrid, Style = Resources["CodeBlock.Border.Style"] as Style };
            case CalloutNode calloutNode:
                var calloutIcon = new Path();
                blockGrid.Children.Add(calloutIcon);
                
                var calloutBorder = new Border { Child = blockGrid };
                
                if (Resources[$"Callout.{calloutNode.Type}.Style"] is Style calloutStyle)
                {
                    calloutBorder.Style = calloutStyle;
                }
                return calloutBorder;
        }

        return blockGrid;
    }
    
    private void AddSpecialBlockElements(IBlockNode blockNode, Grid blockGrid, ItemsControl childrenList)
    { 
        switch (blockNode)
        {   
            case UnorderedListNode:
                blockGrid.Children.Add(new Ellipse { Style = Resources["BulletIndicator.Style"] as Style });
                break;
            case OrderedListNode orderedListNode:
                blockGrid.Children.Add(new TextBlock
                {
                    Text = $"{orderedListNode.ListNumber}.", 
                    Style = Resources["ListIndicator.Style"] as Style
                });
                break;
            case TaskListNode taskListNode:
                if (taskListNode.IsChecked)
                {
                    blockGrid.Resources.Add(typeof(TextBlock), Resources["CheckBox.Checked.Text.Style"]);
                }
                blockGrid.Children.Add(new CheckBox { IsChecked = taskListNode.IsChecked });
                break;
            case IndentedNode:
                childrenList.Margin = new Thickness(left: 19, 0, 0, 0);
                break;
            case HeaderNode headerNode:
                AddHeaderElements(headerNode, blockGrid);
                break;
            case QuoteNode quoteNode:
                AddQuoteElements(quoteNode, blockGrid, childrenList);
                break;
            case ToggleListNode:
                AddToggleListElements(blockGrid, childrenList);
                break;
            case ImageNode imageNode:
                AddImageElements(imageNode, childrenList);
                break;
        }
    }

    private void AddHeaderElements(HeaderNode headerNode, Grid blockGrid)
    {
        if (Resources[$"Header{headerNode.Level}.Style"] is Style headerStyle)
        {
            blockGrid.Resources.Add(typeof(TextBlock), headerStyle); 
        }
        blockGrid.Children.Add(new TextBlock
        {
            Text = new string('#', headerNode.Level), 
            Margin = new Thickness(0, 0, right: 8, 0)
        });
    }

    private void AddQuoteElements(QuoteNode quoteNode, Grid blockGrid, ItemsControl childrenList)
    {
        blockGrid.Resources.Add(typeof(TextBlock), Resources["QuoteBlock.Text.Style"] as Style);
        blockGrid.Children.Add(new Rectangle { Style = Resources["QuoteBlock.Indicator.Style"] as Style });

        if (quoteNode.Author != null)
        {
            var authorBlock = new TextBlock();
            authorBlock.Inlines.Add(new Run($"\n- {quoteNode.Author}") { FontWeight = FontWeights.Bold });
            childrenList.Items.Add(authorBlock);
        }
    }

    private void AddToggleListElements(Grid blockGrid, ItemsControl childrenList)
    {
        var toggleIndicator = new Path();
        var toggleBorder = new Border
        {
            Style = Resources["Toggle.Button.Style"] as Style,
            Child = toggleIndicator
        };
        blockGrid.Children.Add(toggleBorder);

        toggleBorder.PreviewMouseLeftButtonDown += OnToggleClicked;
                
        for (var i = 1; i < childrenList.Items.Count; i++)
        {
            if (childrenList.Items[i] is not FrameworkElement childrenView) continue;
            childrenView.Visibility = Visibility.Collapsed;
        }

        return;
        
        void OnToggleClicked(object o, MouseButtonEventArgs mouseButtonEventArgs)
        {
            for (var i = 1; i < childrenList.Items.Count; i++)
            {
                if (childrenList.Items[i] is FrameworkElement childrenView)
                {
                    childrenView.Visibility = childrenView.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
                }
            }

            var downArrow = Application.Current.Resources["Drawing.DownArrow"] as Geometry;
            var rightArrow = Application.Current.Resources["Drawing.RightArrow"] as Geometry;

            toggleIndicator.Data = toggleIndicator.Data == downArrow ? rightArrow : downArrow;
        }
    }
    
    private void AddImageElements(ImageNode imageNode, ItemsControl childrenList)
    {
        if (TryLoadImage(imageNode.SourceUri, out var image))
        {
            image!.Width = image.Source.Width * imageNode.Scale;
            image.Height = image.Source.Height * imageNode.Scale;
            image.Margin = new Thickness(0, 0, 0, bottom: MarkupViewMargin);
            
            childrenList.Items.Insert(0, image);

            foreach (var item in childrenList.Items)
            {
                if (item is not FrameworkElement view) continue;
                view.HorizontalAlignment = HorizontalAlignment.Center;
            }
            return;
        }

        var errorMessage = string.Format(
            Application.Current.Resources["String.Markup.Image.Error"] as string ?? "",
            imageNode.SourceUri
        );
                
        var errorText = new TextBlock
        {
            Text = errorMessage,
            Foreground = Brushes.Red, 
            Margin = new Thickness(0, 0, 0, bottom: MarkupViewMargin)
        };

        childrenList.Items.Insert(0, errorText);
    }
    
    private Run GetRunFromInline(InlineMarkup inlineMarkup, double paragraphFontSize)
    {
        var inline = new Run(inlineMarkup.Text);

        if (inlineMarkup.LinkUri != null)
        {
            AddLinkToInline(inline, inlineMarkup.LinkUri, OpenDocumentByNameCommand);
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
                    inline.Background = Application.Current.Resources["Brush.Markup.Surface"] as SolidColorBrush;
                    break;
                case InlineMarkupModifiers.Spoiler:
                    inline.Background = Application.Current.Resources["Brush.Markup.Surface"] as SolidColorBrush;
                    inline.Foreground = Application.Current.Resources["Brush.Markup.Surface"] as SolidColorBrush;
                    inline.Cursor = Cursors.Hand;
                    inline.PreviewMouseLeftButtonDown += (_, _) =>
                    {
                        inline.Foreground = previousForeground;
                        if (modifier != InlineMarkupModifiers.Code)
                        {
                            inline.Background = previousBackground;
                        }
                        inline.Cursor = Cursors.Arrow;
                    };
                    break;
            }
        }

        return inline;
    }

    private void AddLinkToInline(Inline inline, string linkUri, ICommand? openDocumentByName)
    {
        inline.TextDecorations.Add(TextDecorations.Underline);

        if (linkUri.StartsWith("doc:"))
        {
            inline.Cursor = Cursors.Hand;
            inline.Foreground = Brushes.Blue;

            var docName = linkUri[4..];
            inline.PreviewMouseLeftButtonDown += (_, _) =>
            {
                openDocumentByName?.Execute(docName);   
            };
        }
        else if (linkUri.StartsWith('@'))
        {
            inline.Cursor = Cursors.Hand;
            inline.Foreground = Brushes.Blue;
            
            var labelName = linkUri.TrimStart('@');
            const int scrollMargin = 400;
            inline.PreviewMouseLeftButtonDown += (_, _) =>
            {
                foreach (var item in MarkupViewerPanel.Items)
                {
                    if (item is not FrameworkElement { Tag: string labelTag } labelView || labelTag != labelName) continue;
                    labelView.BringIntoView(new Rect(0, 0, scrollMargin, scrollMargin));
                    return;
                }
            };
        }
        else if (Uri.TryCreate(linkUri, UriKind.Absolute, out var uri) && uri.Scheme is "http" or "https" or "file")
        {
            inline.Cursor = Cursors.Hand;
            inline.Foreground = Brushes.Blue;
            inline.PreviewMouseLeftButtonDown += (_, _) =>
            {
                try
                {
                    Process.Start(new ProcessStartInfo { FileName = linkUri, UseShellExecute = true });
                }
                catch (Exception e) when (e is InvalidOperationException or Win32Exception)
                {
                    var appResources = Application.Current.Resources;
                    var uriErrorMessage = string.Format(appResources["String.Error.OpenLink"] as string ?? "", linkUri);
                    new MessageBox
                    {
                        Owner = Application.Current.MainWindow,
                        Title = appResources["String.Error"] as string,
                        MessageIconPath = appResources["Drawing.Exclamation"] as Geometry,
                        Message = uriErrorMessage,
                        Options = [new MessageBoxOption(appResources["String.Button.Understood"] as string ?? "")]
                    }.ShowDialog();
                }
            };
        }
        else
        {
            inline.Foreground = Brushes.Gray;
        }
    }
    
    private bool TryLoadImage(string uriText, out Image? image)
    {
        image = null;
        
        if (!Uri.TryCreate(uriText, UriKind.Absolute, out var uri) || uri.Scheme != "file") return false;
        try
        {
            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.UriSource = uri;
            bitmapImage.EndInit();
            
            image = new Image { Source = bitmapImage };
            return true;
        }
        catch (Exception e) when (e is FileNotFoundException or DirectoryNotFoundException or UnauthorizedAccessException) { }
        
        return false;
    }
}
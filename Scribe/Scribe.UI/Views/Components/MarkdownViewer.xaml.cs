using System.Windows;
using System.Windows.Controls;
using Scribe.Markdown.Nodes;

namespace Scribe.UI.Views.Components;

public partial class MarkdownViewer : UserControl
{
    public MarkdownViewer() => InitializeComponent();
    
    public IMarkdownNode DocumentRoot
    {
        get => (IMarkdownNode) GetValue(DocumentRootProperty);
        set => SetValue(DocumentRootProperty, value);
    }
    
    public static readonly DependencyProperty DocumentRootProperty = DependencyProperty.Register(
        name: nameof(DocumentRoot),
        propertyType: typeof(IMarkdownNode),
        ownerType: typeof(MarkdownViewer),
        new FrameworkPropertyMetadata(propertyChangedCallback: OnDocumentRootChanged)
    );

    private static void OnDocumentRootChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var markdownViewer = (MarkdownViewer) d;
        var documentRoot = (IMarkdownNode?) e.NewValue;

        markdownViewer.ViewerPanel.Children.Clear();

        if (documentRoot == null) return;
        
        foreach (var node in documentRoot.Children)
        {
            if (node is ParagraphNode paragraphNode)
            {
                var paragraphTextBlock = new TextBlock
                {
                    Text = paragraphNode.Text, 
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(0, markdownViewer.ViewerPanel.Children.Count == 0 ? 0 : 6, 0, 6)
                };
                
                markdownViewer.ViewerPanel.Children.Add(paragraphTextBlock);
            }
        }
    }
}
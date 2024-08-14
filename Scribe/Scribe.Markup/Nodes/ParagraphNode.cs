using Scribe.Markup.Inlines;

namespace Scribe.Markup.Nodes;

public class ParagraphNode : IMarkupNode
{
    public string RawText { get; set; } = "";
    
    public ICollection<IMarkupNode> Children { get; } = [];
    
    public ICollection<InlineMarkup> Inlines { get; } = [];
}
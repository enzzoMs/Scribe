using Scribe.Markup.Inlines;

namespace Scribe.Markup.Nodes.Leafs;

public class ParagraphNode : ILeafNode
{
    public string RawText { get; set; } = "";
    
    public ICollection<InlineMarkup> Inlines { get; } = [];
}
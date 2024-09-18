namespace Scribe.Markup.Nodes.Blocks;

public class QuoteNode(string? author = null) : IBlockNode
{
    public string? Author { get; } = author;
    
    public ICollection<IMarkupNode> Children { get; } = [];
}
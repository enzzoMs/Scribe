namespace Scribe.Markup.Nodes.Blocks;

public class QuoteNode(string? author = null) : IBlockNode
{
    public string? Author { get; } = author;
    
    public List<IMarkupNode> Children { get; } = [];
}
namespace Scribe.Markup.Nodes.Blocks;

public class UnorderedListNode : IBlockNode
{
    public List<IMarkupNode> Children { get; } = [];
}
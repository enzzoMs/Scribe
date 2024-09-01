namespace Scribe.Markup.Nodes.Blocks;

public class UnorderedListNode : IBlockNode
{
    public ICollection<IMarkupNode> Children { get; } = [];
}
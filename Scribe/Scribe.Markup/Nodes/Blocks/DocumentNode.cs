namespace Scribe.Markup.Nodes.Blocks;

public class DocumentNode : IBlockNode
{
    public ICollection<IMarkupNode> Children { get; } = [];
}
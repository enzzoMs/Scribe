namespace Scribe.Markup.Nodes.Blocks;

public interface IBlockNode : IMarkupNode
{
    public List<IMarkupNode> Children { get; }
}
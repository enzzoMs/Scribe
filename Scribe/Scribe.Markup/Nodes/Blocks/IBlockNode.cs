namespace Scribe.Markup.Nodes.Blocks;

public interface IBlockNode : IMarkupNode
{
    public ICollection<IMarkupNode> Children { get; }
}
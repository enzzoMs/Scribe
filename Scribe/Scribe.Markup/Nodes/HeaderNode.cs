namespace Scribe.Markup.Nodes;

public class HeaderNode(int level) : IMarkupNode
{
    public int Level { get; } = level;

    public ICollection<IMarkupNode> Children { get; } = [];
}
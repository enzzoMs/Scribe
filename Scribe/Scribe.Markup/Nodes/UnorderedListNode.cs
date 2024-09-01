namespace Scribe.Markup.Nodes;

public class UnorderedListNode : IMarkupNode
{
    public ICollection<IMarkupNode> Children { get; } = [];
}
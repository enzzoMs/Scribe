namespace Scribe.Markup.Nodes;

public class DocumentNode : IMarkupNode
{
    public ICollection<IMarkupNode> Children { get; } = [];
}
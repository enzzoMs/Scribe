namespace Scribe.Markup.Nodes;

public interface IMarkupNode
{
    public ICollection<IMarkupNode> Children { get; }
}
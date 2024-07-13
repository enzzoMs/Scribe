namespace Scribe.Markdown.Nodes;

public interface IMarkdownNode
{
    public ICollection<IMarkdownNode> Children { get; }
}
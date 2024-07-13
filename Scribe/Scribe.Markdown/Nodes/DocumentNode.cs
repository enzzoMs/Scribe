namespace Scribe.Markdown.Nodes;

public class DocumentNode : IMarkdownNode
{
    public ICollection<IMarkdownNode> Children { get; } = [];
}
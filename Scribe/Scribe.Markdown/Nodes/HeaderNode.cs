namespace Scribe.Markdown.Nodes;

public class HeaderNode(int level) : IMarkdownNode
{
    public int Level { get; } = level;

    public ICollection<IMarkdownNode> Children { get; } = [];
}
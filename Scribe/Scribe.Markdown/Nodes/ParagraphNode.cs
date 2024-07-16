namespace Scribe.Markdown.Nodes;

public class ParagraphNode : IMarkdownNode
{
    public string Text { get; set; } = "";

    public ICollection<IMarkdownNode> Children { get; } = [];
}
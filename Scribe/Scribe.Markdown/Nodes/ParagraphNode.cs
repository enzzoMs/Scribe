namespace Scribe.Markdown.Nodes;

public class ParagraphNode : IMarkdownNode
{
    public ICollection<IMarkdownNode> Children { get; } = [];
    public string Text { get; set; } = "";
}
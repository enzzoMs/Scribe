using System.Text.RegularExpressions;
using Scribe.Markdown.Nodes;

namespace Scribe.Markdown;

public static class MarkdownParser
{
    public static DocumentNode Parse(string documentText)
    {
        var documentRoot = new DocumentNode();

        if (string.IsNullOrWhiteSpace(documentText))
        {
            return documentRoot;
        }

        var openNodes = new List<IMarkdownNode> { documentRoot };
        
        foreach (var line in documentText.Split("\n"))
        {
            switch (openNodes.Last())
            {
                case DocumentNode documentNode:
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        var newParagraphNode = new ParagraphNode
                        {
                            Text = Regex.Replace(line, "\\s+", " ").Trim()
                        };
                        
                        documentNode.Children.Add(newParagraphNode);
                        openNodes.Add(newParagraphNode);
                    }
                    break;
                case ParagraphNode paragraphNode:
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        openNodes.Remove(paragraphNode);
                    }
                    else
                    {
                        paragraphNode.Text += $" {Regex.Replace(line, "\\s+", " ").Trim()}";
                    }
                    break;
            }
        }

        return documentRoot;
    }
}
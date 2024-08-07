using System.Text.RegularExpressions;
using Scribe.Markdown.Nodes;

namespace Scribe.Markdown;

public static class MarkdownParser
{
    private static readonly Regex HeadingPattern = new(@"^(#+)\s(.+)$", RegexOptions.Compiled);

    public static DocumentNode Parse(string documentText)
    {
        var documentRoot = new DocumentNode();

        if (string.IsNullOrWhiteSpace(documentText))
        {
            return documentRoot;
        }
        
        var openNodes = new List<IMarkdownNode> { documentRoot };

        foreach (var docLine in documentText.Split("\n"))
        {
            openNodes.RemoveAll(node => 
                (string.IsNullOrWhiteSpace(docLine) || HeadingPattern.IsMatch(docLine)) && node is ParagraphNode or HeaderNode
            );

            if (string.IsNullOrWhiteSpace(docLine))
            {
                continue;
            }
            
            var lineWithoutMarkup = docLine;

            if (HeadingPattern.IsMatch(docLine))
            {
                var newHeader = new HeaderNode(
                    level: docLine.TakeWhile(c => c == '#').Count()
                );
                
                openNodes.Last().Children.Add(newHeader);
                openNodes.Add(newHeader);

                lineWithoutMarkup = docLine.TrimStart('#').Trim();
            } 
            
            if (openNodes.Last() is ParagraphNode paragraphNode)
            {
                paragraphNode.Text += $" {Regex.Replace(lineWithoutMarkup, "\\s+", " ").Trim()}";
            }
            else
            {
                var newParagraphNode = new ParagraphNode
                {
                    Text = Regex.Replace(lineWithoutMarkup, "\\s+", " ").Trim()
                };
                
                openNodes.Last().Children.Add(newParagraphNode);
                openNodes.Add(newParagraphNode);
            }
        }

        return documentRoot;
    }
}
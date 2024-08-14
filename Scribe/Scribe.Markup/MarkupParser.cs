using System.Text.RegularExpressions;
using Scribe.Markup.Inlines;
using Scribe.Markup.Nodes;

namespace Scribe.Markup;

public static class MarkupParser
{
    private static readonly Regex HeadingPattern = new(@"^(=+)\s(.+)$", RegexOptions.Compiled);
    private static readonly Regex InlineMarkupPattern = new(@"{(.+?)}(\[(.*?)\])", RegexOptions.Compiled);
    private static readonly Regex InlineInnerTextPattern = new("(?<={)(.+)(?=})", RegexOptions.Compiled);
    private static readonly Regex InlineModifiersPattern = new(@"(?<=\[)(.+?)(?=\])", RegexOptions.Compiled);

    public static DocumentNode ParseText(string documentText)
    {
        var documentRoot = new DocumentNode();

        if (string.IsNullOrWhiteSpace(documentText))
        {
            return documentRoot;
        }
        
        var openNodes = new List<IMarkupNode> { documentRoot };
        var paragraphs = new List<ParagraphNode>();
        
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
                    level: docLine.TakeWhile(c => c == '=').Count()
                );
                
                openNodes.Last().Children.Add(newHeader);
                openNodes.Add(newHeader);

                lineWithoutMarkup = docLine.TrimStart('=').Trim();
            } 
            
            if (openNodes.Last() is ParagraphNode paragraphNode)
            {
                paragraphNode.RawText += $" {Regex.Replace(lineWithoutMarkup, "\\s+", " ").Trim()}";
            }
            else
            {
                var newParagraphNode = new ParagraphNode
                {
                    RawText = Regex.Replace(lineWithoutMarkup, "\\s+", " ").Trim()
                };
                
                openNodes.Last().Children.Add(newParagraphNode);
                openNodes.Add(newParagraphNode);
                paragraphs.Add(newParagraphNode);
            }
        }

        foreach (var paragraph in paragraphs)
        {
            ParseInlines(paragraph);
        }
        
        return documentRoot;
    }

    private static void ParseInlines(ParagraphNode paragraphNode)
    {
        var inlineMatches = InlineMarkupPattern.Matches(paragraphNode.RawText);
        
        if (inlineMatches.Count == 0)
        {
            paragraphNode.Inlines.Add(new InlineMarkup(paragraphNode.RawText));
            return;
        }

        Match? previousInline = null;

        foreach (Match currentInline in inlineMatches)
        {
            if (previousInline?.Index != currentInline.Index)
            {
                paragraphNode.Inlines.Add(new InlineMarkup(
                    paragraphNode.RawText[(previousInline?.Index + previousInline?.Length ?? 0)..currentInline.Index]
                ));
            }

            var inlineText = InlineInnerTextPattern.Match(currentInline.Value).Value;
            var newInline = new InlineMarkup(inlineText);
            
            var modifiers = InlineModifiersPattern.Match(
                currentInline.Value[(inlineText.Length + 1)..]
            ).Value.Split(",");

            foreach (var modifier in modifiers)
            {
                switch (modifier.Trim())
                {
                    case "b":
                        newInline.Modifiers.Add(InlineMarkupModifiers.Bold);
                        break;
                    case "i":
                        newInline.Modifiers.Add(InlineMarkupModifiers.Italic);
                        break;
                    case "u":
                        newInline.Modifiers.Add(InlineMarkupModifiers.Underline);
                        break;
                    case "s":
                        newInline.Modifiers.Add(InlineMarkupModifiers.Strikethrough);
                        break;
                    case "super":
                        newInline.Modifiers.Add(InlineMarkupModifiers.Superscript);
                        break;
                    case "sub":
                        newInline.Modifiers.Add(InlineMarkupModifiers.Subscript);
                        break;
                }
            }
            
            paragraphNode.Inlines.Add(newInline);
            
            previousInline = currentInline;
        }

        var inlineEndIndex = previousInline?.Index + previousInline?.Length;
        
        if (inlineEndIndex != null && inlineEndIndex != paragraphNode.RawText.Length)
        {
            paragraphNode.Inlines.Add(new InlineMarkup(
                paragraphNode.RawText[inlineEndIndex.Value..]
            ));
        }
    }
}
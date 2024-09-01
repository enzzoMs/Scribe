using System.Text.RegularExpressions;
using Scribe.Markup.Inlines;
using Scribe.Markup.Nodes;

namespace Scribe.Markup;

public static class MarkupParser
{
    private static readonly Regex BlockMarkupPattern = new(@"(?<=^\s*\[).+?(?=\]\s.+$)", RegexOptions.Compiled);
    private static readonly Regex OrderedListPattern = new("[0-9][0-9]*.", RegexOptions.Compiled);

    private static readonly Regex BlockMultilineMarkupPattern = new(@"(?<=^\s*\[).+?(?=\]%\s*$)", RegexOptions.Compiled);

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
        
        ParagraphNode? openParagraph = null;
        var openBlocks = new List<IMarkupNode> { documentRoot };
        
        var paragraphs = new List<ParagraphNode>();

        foreach (var docLine in documentText.Split("\n"))
        {
            if (string.IsNullOrWhiteSpace(docLine))
            {
                openParagraph = null;
                continue;
            }

            var lineWithoutMarkup = Regex.Replace(docLine, "\\s+", " ").Trim();

            var blockMatch = BlockMarkupPattern.Match(lineWithoutMarkup);
            var blockMultilineMatch = BlockMultilineMarkupPattern.Match(lineWithoutMarkup);

            var markupNodeType = blockMatch.Success ? blockMatch.Value.Trim() : blockMultilineMatch.Value.Trim();
            var newBlockNode = GetNodeFromMarkup(markupNodeType);
            
            if (newBlockNode != null)
            {
                openBlocks.Last().Children.Add(newBlockNode);    
                openParagraph = null;
                
                if (blockMultilineMatch.Success)
                {
                    openBlocks.Add(newBlockNode);
                    continue;
                }
                
                if (blockMatch.Success)
                {
                    lineWithoutMarkup = lineWithoutMarkup[(blockMatch.Length + 2)..].Trim();
                } 
            }
            
            if (openBlocks.Count > 1 && lineWithoutMarkup == "%")
            {
                openBlocks.RemoveAt(openBlocks.Count - 1);
                continue;
            }
            
            if (openParagraph != null)
            {
                openParagraph.RawText += " " + lineWithoutMarkup;
            }
            else
            {
                var newParagraphNode = new ParagraphNode { RawText = lineWithoutMarkup };

                if (newBlockNode != null)
                {
                    newBlockNode.Children.Add(newParagraphNode);
                }
                else
                {
                    openBlocks.Last().Children.Add(newParagraphNode);
                }
                
                openParagraph = newParagraphNode;
                paragraphs.Add(newParagraphNode);
            }
        }

        foreach (var paragraph in paragraphs)
        {
            ParseInlines(paragraph);
        }
        
        return documentRoot;
    }

    private static IMarkupNode? GetNodeFromMarkup(string markup)
    {
        if (OrderedListPattern.IsMatch(markup))
        {
            return new OrderedListNode(listNumber: int.Parse(markup.Trim('.')));
        }
        
        return markup switch
        {
            "#" or "##" or "###" or "####" or "#####" or "######" => new HeaderNode(
                level: markup.TakeWhile(c => c == '#').Count()
            ),
            "*" => new UnorderedListNode(),
            _ => null
        };
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
using System.Drawing;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Scribe.Markup.Inlines;
using Scribe.Markup.Nodes.Blocks;
using Scribe.Markup.Nodes.Leafs;

namespace Scribe.Markup;

public static class MarkupParser
{
    // Pattern for a markup node. E.g. [quote], [quote]% 
    private static readonly Regex MarkupNodePattern = new(@"(?<=^\[)[^\]]*?[^\\](?=\](%?| .*)$)", RegexOptions.Compiled);
    
    // Pattern for ordered lists. E.g. 1., 20., 360.
    private static readonly Regex OrderedListPattern = new("^[0-9][0-9]*.$", RegexOptions.Compiled);
    
    // Pattern for divider. E.g. =====
    private static readonly Regex DividerPattern = new("^=+$", RegexOptions.Compiled);
    
    // Pattern for images. E.g. img(50%)=imgLink, img=imgLink 
    private static readonly Regex ImagePattern = new(@"img(\([0-9]+%\))?=(.+)", RegexOptions.Compiled);
    
    // Pattern for progress bar. E.g. oooo.....
    private static readonly Regex ProgressBarPattern = new(@"^((o+\.*)|(o*\.+))$", RegexOptions.Compiled);

    // Pattern for label. E.g. @label=example
    private static readonly Regex LabelPattern = new("^@label=(.+)$", RegexOptions.Compiled);

    // Pattern for inline markup. E.g. {text}[b,i]
    private static readonly Regex InlineMarkupPattern = new(@"{([^}\n]+?)}(\[\]|\[([^\]\n]*?[^\\])\])", RegexOptions.Compiled);
    
    // Pattern for inline colors. E.g. (foreg=#123456), (backg=#FF123456)
    private static readonly Regex InlineColorPattern = new(
        "(foreg|backg)=#(([0-9a-fA-F]{8}|[0-9a-fA-F]{6})|[a-z]+)", RegexOptions.Compiled
    );

    private static readonly Regex EmptySpacePattern = new(@"\s+", RegexOptions.Compiled);

    public static DocumentNode ParseText(string documentText)
    {
        var documentRoot = new DocumentNode();

        if (string.IsNullOrWhiteSpace(documentText))
        {
            return documentRoot;
        }
        
        ParagraphNode? openParagraph = null;
        var openBlocks = new List<IBlockNode> { documentRoot };
        
        var paragraphs = new List<ParagraphNode>();

        foreach (var docLine in documentText.Split("\n"))
        {
            var inCodeBlock = openBlocks.Last() is CodeNode;

            if (!inCodeBlock && string.IsNullOrWhiteSpace(docLine))
            {
                openParagraph = null;
                continue;
            }

            var lineWithoutMarkup = EmptySpacePattern.Replace(docLine, " ").Trim();

            if (openBlocks.Count > 1 && lineWithoutMarkup == "%")
            {
                openBlocks.RemoveAt(openBlocks.Count - 1);
                continue;
            }
            
            if (!inCodeBlock && DividerPattern.IsMatch(lineWithoutMarkup))
            {
                openBlocks.Last().Children.Add(new DividerNode(length: lineWithoutMarkup.Length));    
                openParagraph = null;
                continue;
            }

            var labelMatch = LabelPattern.Match(lineWithoutMarkup);

            if (!inCodeBlock && labelMatch.Success)
            {
                var newLabelNode = new LabelNode(name: labelMatch.Groups[1].Value);
                if (openBlocks.Count > 1)
                {
                    documentRoot.Children.Insert(documentRoot.Children.Count - 1, newLabelNode);
                }
                else
                {
                    documentRoot.Children.Add(newLabelNode);
                }
                continue;
            }
            
            var nodeMatch = MarkupNodePattern.Match(lineWithoutMarkup);

            var newLeafNode = GetLeafNodeFromMarkup(nodeMatch.Value);
            
            if (!inCodeBlock && newLeafNode != null)
            {
                openBlocks.Last().Children.Add(newLeafNode);    
                openParagraph = null;
                continue;
            }
            
            var newBlockNode = GetBlockNodeFromMarkup(nodeMatch.Value.Trim());
        
            if (!inCodeBlock && newBlockNode != null)
            {
                openBlocks.Last().Children.Add(newBlockNode);    
                openParagraph = null;
            
                if (lineWithoutMarkup.Last() == '%')
                {
                    openBlocks.Add(newBlockNode);
                    continue;
                }
                
                lineWithoutMarkup = lineWithoutMarkup[(nodeMatch.Length + 2)..].Trim();
            }
            
            if (string.IsNullOrWhiteSpace(lineWithoutMarkup)) continue;
            
            var lineWithoutEscapeCharacters = new StringBuilder(lineWithoutMarkup)
                .Replace(@"\\", @"\")
                .Replace(@"\]", "]")
                .Replace(@"\[", "[")
                .Replace(@"\}", "}")
                .Replace(@"\{", "{")
                .Replace(@"\/", "/")
                .Replace(@"\%", "%");
            
            if (!inCodeBlock)
            {
                lineWithoutEscapeCharacters.Replace("///", "\n");
            }

            lineWithoutMarkup = lineWithoutEscapeCharacters.ToString();
                
            if (openParagraph != null)
            {
                openParagraph.RawText += inCodeBlock ? $"\n{docLine.TrimEnd('\r')}" : 
                    (openParagraph.RawText.Last() == '\n' ? "" : " ") + lineWithoutMarkup;
            }
            else
            {
                var newParagraphNode = new ParagraphNode
                {
                    RawText = inCodeBlock ? docLine.TrimEnd('\r') : lineWithoutMarkup
                };

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

    private static IBlockNode? GetBlockNodeFromMarkup(string markup)
    {
        if (markup.StartsWith("quote="))
        {
            return new QuoteNode(author: markup[6..]);
        }

        if (markup.StartsWith("::"))
        {
            CalloutType? calloutType = markup[2..] switch
            {
                "callout" => CalloutType.Default,
                "favorite" => CalloutType.Favorite,
                "question" => CalloutType.Question,
                "success" => CalloutType.Success,
                "failure" => CalloutType.Failure,
                "warning" => CalloutType.Warning,
                "note" => CalloutType.Note,
                _ => null
            };

            return calloutType == null ? null : new CalloutNode(calloutType.Value);
        }
        
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
            "quote" => new QuoteNode(),
            "code" => new CodeNode(),
            "toggle" => new ToggleListNode(),
            ">>" => new IndentedNode(),
            "-" or "x" => new TaskListNode(isChecked: markup == "x"),
            _ => null
        };
    }

    private static ILeafNode? GetLeafNodeFromMarkup(string markup)
    {
        if (ProgressBarPattern.IsMatch(markup))
        {
            return new ProgressBarNode(
                maxLength: markup.Length, length: markup.TakeWhile(c => c == 'o').Count()
            );
        }
            
        var imageMatch = ImagePattern.Match(markup);

        if (!imageMatch.Success) return null;
        
        var imageParts = imageMatch.Groups;

        var imageScale = imageParts[1].Value.TrimStart('(').TrimEnd(')', '%');
        var imageUri = imageParts[2].Value;

        var imageNode = new ImageNode(imageUri);
            
        if (int.TryParse(imageScale, out var scalePercentage))
        {
            imageNode.Scale = scalePercentage / 100.0;
        }

        return imageNode;
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

            var inlineText = currentInline.Groups[1].Value;
            var modifiers = currentInline.Groups[3].Value.Split(',');
            
            var newInline = new InlineMarkup(inlineText);
            
            foreach (var modifier in modifiers)
            {
                var trimmedModifier = modifier.Trim();

                if (trimmedModifier.StartsWith("link="))
                {
                    newInline.LinkUri = trimmedModifier[5..];
                    continue;
                }
                
                if (InlineColorPattern.IsMatch(trimmedModifier))
                {
                    var colorModifier = trimmedModifier.Split("=#");
                    var colorType = colorModifier.First();
                    var colorText = colorModifier.Last();

                    var color = GetColorByName(colorText);

                    if (color == null)
                    {
                        // Adding the alpha channel if needed
                        colorText = colorText.Length == 6 ? "FF" + colorText : colorText;

                        if (int.TryParse(colorText, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out var colorNum))
                        {
                            color = Color.FromArgb(colorNum);
                        }
                    }

                    if (colorType == "foreg")
                    {
                        newInline.Foreground = color;
                    }
                    else
                    {
                        newInline.Background = color;
                    }
                    
                    continue;
                }
                
                switch (trimmedModifier)
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
                    case "code":
                        newInline.Modifiers.Add(InlineMarkupModifiers.Code);
                        break;
                    case "spoiler":
                        newInline.Modifiers.Add(InlineMarkupModifiers.Spoiler);
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

    private static Color? GetColorByName(string colorName) => colorName switch
    {
        "black" => Color.Black,
        "white" => Color.White,
        "gray" => Color.Gray,
        "orange" => Color.Orange,
        "yellow" => Color.Yellow,
        "green" => Color.Green,
        "blue" => Color.Blue,
        "purple" => Color.Purple,
        "pink" => Color.Pink,
        "red" => Color.Red,
        _ => null
    };
}
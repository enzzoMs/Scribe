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
    private static readonly Regex MarkupNodePattern = new(@"(?<=^\$?\[)[^\]]+?(?=\](%?| .*)$)", RegexOptions.Compiled);
    
    // Pattern for ordered lists. E.g. 1., 20., 360.
    private static readonly Regex OrderedListPattern = new("^[0-9][0-9]*.$", RegexOptions.Compiled);
    
    // Pattern for divider. E.g. =====
    private static readonly Regex DividerPattern = new(@"^\$?=+$", RegexOptions.Compiled);
    
    // Pattern for images. E.g. img(50%)=imgLink, img=imgLink 
    private static readonly Regex ImagePattern = new(@"img(\([0-9]+%\))?=(.+)", RegexOptions.Compiled);
    
    // Pattern for progress bar. E.g. (oooo.....)
    private static readonly Regex ProgressBarPattern = new(@"^\$?\(((o+\.*)|(o*\.+))\)$", RegexOptions.Compiled);

    // Pattern for label. E.g. @label=example
    private static readonly Regex LabelPattern = new(@"^\$?@label=(.+)$", RegexOptions.Compiled);

    // Pattern for inline markup. E.g. {text}[b,i]
    private static readonly Regex InlineMarkupPattern = new(@"\$?{([^}\n]*?)}\[([^\]\n]*?)\]", RegexOptions.Compiled);
    
    // Pattern for inline colors. E.g. (foreg=#123456), (backg=#FF123456)
    private static readonly Regex InlineColorPattern = new(
        "(foreg|backg)=#(([0-9a-fA-F]{8}|[0-9a-fA-F]{6})|[a-z]+)", RegexOptions.Compiled
    );

    // Pattern for new lines ( /// )
    private static readonly Regex NewLineMarkupPattern = new(@"(?<!\$)\/\/\/", RegexOptions.Compiled);

    private static readonly Regex EmptySpacePattern = new(@"\s+", RegexOptions.Compiled);

    private static readonly Regex IgnoreMarkupPattern = new(@"(?<!\$)\$(?!\$)", RegexOptions.Compiled);

    public static DocumentNode ParseText(string documentText)
    {
        var documentRoot = new DocumentNode();

        if (string.IsNullOrWhiteSpace(documentText))
        {
            return documentRoot;
        }
        
        ParagraphNode? openParagraph = null;
        var paragraphs = new List<ParagraphNode>();

        var openBlocks = new Stack<IBlockNode>();
        openBlocks.Push(documentRoot);
        
        var tablesGridConfiguration = new Stack<(int Row, int Column)>();

        foreach (var docLine in documentText.Split("\n"))
        {
            var inCodeBlock = openBlocks.Peek() is CodeNode;
            var inTableBlock = openBlocks.Peek() is TableNode;

            if (!inCodeBlock && string.IsNullOrWhiteSpace(docLine))
            {
                openParagraph = null;
                continue;
            }

            var trimmedLine = EmptySpacePattern.Replace(docLine, " ").Trim();
            
            if (openBlocks.Count > 1 && trimmedLine == "%")
            {
                if (openBlocks.Peek() is TableNode)
                {
                    tablesGridConfiguration.Pop();
                }
                openBlocks.Pop();
                openParagraph = null;
                continue;
            }
            
            var newLeafNode = inCodeBlock ? null : GetLeafNodeFromMarkup(trimmedLine);
            if (newLeafNode != null)
            {
                if (newLeafNode is LabelNode)
                {
                    if (openBlocks.Count > 1)
                    {
                        documentRoot.Children.Insert(documentRoot.Children.Count - 1, newLeafNode);
                    }
                    else
                    {
                        documentRoot.Children.Add(newLeafNode);
                    }
                }
                else if (inTableBlock && newLeafNode is DividerNode)
                {
                    var (currentRow, _) = tablesGridConfiguration.Pop();
                    tablesGridConfiguration.Push((Row: currentRow + 1, Column: 0));
                }
                else
                {
                    openBlocks.Peek().Children.Add(newLeafNode);    
                }
                
                openParagraph = null;
                continue;  
            }
            
            var lineWithoutMarkup = trimmedLine;

            IBlockNode? newBlockNode = null;
            var blockNodeMatch = MarkupNodePattern.Match(trimmedLine);

            if (!inCodeBlock && blockNodeMatch.Success && !trimmedLine.StartsWith('$'))
            {
                if (inTableBlock && blockNodeMatch.Value == "cell")
                {
                    var currentTableConfiguration = tablesGridConfiguration.Pop();
                    newBlockNode = new TableCellNode(currentTableConfiguration.Row, currentTableConfiguration.Column);
                    tablesGridConfiguration.Push(currentTableConfiguration with
                    {
                        Column = currentTableConfiguration.Column + 1
                    });
                }
                else
                {
                    newBlockNode = GetBlockNodeFromMarkup(blockNodeMatch.Value.Trim());
                }

                if (newBlockNode is TableNode)
                {
                    tablesGridConfiguration.Push((Row: 0, Column: 0));
                }
                else if (inTableBlock && newBlockNode is not null and not TableCellNode)
                {
                    continue;
                }
        
                if (newBlockNode != null)
                {
                    openBlocks.Peek().Children.Add(newBlockNode);    
                    openParagraph = null;
            
                    if (trimmedLine.Last() == '%')
                    {
                        openBlocks.Push(newBlockNode);
                        continue;
                    }
                
                    lineWithoutMarkup = trimmedLine[(blockNodeMatch.Length + 2)..].Trim();
                    
                    if (string.IsNullOrWhiteSpace(lineWithoutMarkup)) continue;
                }
            }
            
            var paragraphText = inCodeBlock ? 
                docLine.TrimEnd('\r').Replace("\t", "    ") : lineWithoutMarkup;
            
            if (openParagraph != null)
            {
                openParagraph.RawText += inCodeBlock ? $"\n{paragraphText}" : 
                    (openParagraph.RawText.Last() == '\n' ? "" : " ") + paragraphText;
            }
            else
            {
                var newParagraphNode = new ParagraphNode { RawText = paragraphText };

                if (newBlockNode != null)
                {
                    newBlockNode.Children.Add(newParagraphNode);
                }
                else if (openBlocks.Peek() is not TableNode)
                {
                    openBlocks.Peek().Children.Add(newParagraphNode);
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
        
        var imageMatch = ImagePattern.Match(markup);

        if (imageMatch.Success)
        {
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
            "table" => new TableNode(),
            "-" or "x" => new TaskListNode(isChecked: markup == "x"),
            _ => null
        };
    }

    private static ILeafNode? GetLeafNodeFromMarkup(string markup)
    {
        if (markup.StartsWith('$')) return null;
        
        if (ProgressBarPattern.IsMatch(markup))
        {
            var barInnerText = markup.Trim('(', ')');
            
            return new ProgressBarNode(
                maxLength: barInnerText.Length, length: barInnerText.TakeWhile(c => c == 'o').Count()
            );
        }

        if (DividerPattern.IsMatch(markup))
        {
            return new DividerNode(length: markup.Length);
        }

        var labelMatch = LabelPattern.Match(markup);

        return labelMatch.Success ? new LabelNode(name: labelMatch.Groups[1].Value) : null;
    }

    private static void ParseInlines(ParagraphNode paragraphNode)
    {
        var inlineMatches = InlineMarkupPattern.Matches(paragraphNode.RawText);
        
        if (inlineMatches.Count == 0)
        {
            paragraphNode.Inlines.Add(new InlineMarkup(RemoveSpecialCharactersFromText(paragraphNode.RawText)));
            return;
        }

        Match? previousInline = null;

        foreach (Match currentInline in inlineMatches)
        {
            if (previousInline?.Index != currentInline.Index)
            {
                var inlineText = RemoveSpecialCharactersFromText(
                    paragraphNode.RawText[(previousInline?.Index + previousInline?.Length ?? 0)..currentInline.Index]
                );
                paragraphNode.Inlines.Add(new InlineMarkup(inlineText));
            }
            
            if (currentInline.Value.StartsWith('$'))
            {
                var inlineText = RemoveSpecialCharactersFromText(paragraphNode.RawText[
                    (currentInline.Index + 1)..(currentInline.Index + currentInline.Length)
                ]);
                paragraphNode.Inlines.Add(new InlineMarkup(inlineText));
                previousInline = currentInline;
                continue;
            }

            var inlineInnerText = currentInline.Groups[1].Value;
            var modifiers = currentInline.Groups[2].Value.Split(',');
            
            var newInline = new InlineMarkup(RemoveSpecialCharactersFromText(inlineInnerText));
            
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
                RemoveSpecialCharactersFromText(paragraphNode.RawText[inlineEndIndex.Value..])
            ));
        }
    }

    private static string RemoveSpecialCharactersFromText(string text)
    {
        var textWithoutSpecialChars = NewLineMarkupPattern.Replace(text, "\n");
        textWithoutSpecialChars = IgnoreMarkupPattern.Replace(textWithoutSpecialChars, "");
        textWithoutSpecialChars = textWithoutSpecialChars.Replace("$$", "$");

        return textWithoutSpecialChars;
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
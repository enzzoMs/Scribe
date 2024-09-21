using System.Drawing;

namespace Scribe.Markup.Inlines;

public class InlineMarkup(string text)
{
    public string Text { get; } = text;

    public Color? Foreground { get; set; }
    
    public Color? Background { get; set; }

    public string? LinkUri { get; set; }

    public ICollection<InlineMarkupModifiers> Modifiers { get; } = [];
}
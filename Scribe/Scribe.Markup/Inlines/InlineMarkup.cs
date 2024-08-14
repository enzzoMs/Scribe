namespace Scribe.Markup.Inlines;

public class InlineMarkup(string text)
{
    public string Text { get; } = text;

    public ICollection<InlineMarkupModifiers> Modifiers { get; } = [];
}
namespace Scribe.Markup.Nodes;

public class OrderedListNode(int listNumber) : IMarkupNode
{
    public int ListNumber { get; } = listNumber;
    
    public ICollection<IMarkupNode> Children { get; } = [];
}
namespace Scribe.Markup.Nodes.Blocks;

public class CalloutNode(CalloutType type) : IBlockNode
{
    public CalloutType Type { get; } = type;
    
    public List<IMarkupNode> Children { get; } = [];
}

public enum CalloutType
{
    Default,
    Question,
    Favorite,
    Success,
    Failure,
    Warning,
    Note
}
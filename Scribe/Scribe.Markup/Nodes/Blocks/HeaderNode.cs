namespace Scribe.Markup.Nodes.Blocks;

public class HeaderNode(int level) : IBlockNode
{
    private const int MaxHeaderLevel = 6;
    
    public int Level { get; } = level <= 0 ? 1 : level > MaxHeaderLevel ? MaxHeaderLevel : level;

    public List<IMarkupNode> Children { get; } = [];
}
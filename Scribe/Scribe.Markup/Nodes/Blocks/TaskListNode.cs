namespace Scribe.Markup.Nodes.Blocks;

public class TaskListNode(bool isChecked) : IBlockNode
{
    public bool IsChecked { get; } = isChecked;
    
    public List<IMarkupNode> Children { get; } = [];
}
namespace Scribe.Markup.Nodes.Blocks;

public class TableCellNode(int rowNumber, int columnNumber) : IBlockNode
{
    public int RowNumber { get; } = rowNumber < 0 ? 0 : rowNumber;
    
    public int ColumnNumber { get; } = columnNumber < 0 ? 0 : columnNumber;
    
    public List<IMarkupNode> Children { get; } = [];
}
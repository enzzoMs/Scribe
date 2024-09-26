namespace Scribe.Markup.Nodes.Blocks;

public class ImageNode(string sourceUri) : IBlockNode
{
    public double Scale { get; set; } = 1.0;

    public string SourceUri { get; } = sourceUri;
    
    public List<IMarkupNode> Children { get; } = [];
}
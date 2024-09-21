namespace Scribe.Markup.Nodes.Blocks;

public class ImageNode(string sourceUri) : IBlockNode
{
    public string SourceUri { get; } = sourceUri;

    public double Scale { get; set; } = 1.0;
    
    public ICollection<IMarkupNode> Children { get; } = [];
}
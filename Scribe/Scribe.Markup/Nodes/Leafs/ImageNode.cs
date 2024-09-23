namespace Scribe.Markup.Nodes.Leafs;

public class ImageNode(string sourceUri) : ILeafNode
{
    public string SourceUri { get; } = sourceUri;

    public double Scale { get; set; } = 1.0;
}
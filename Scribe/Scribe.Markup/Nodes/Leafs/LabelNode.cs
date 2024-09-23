namespace Scribe.Markup.Nodes.Leafs;

public class LabelNode(string name) : ILeafNode
{
    public string Name { get; } = name;
}
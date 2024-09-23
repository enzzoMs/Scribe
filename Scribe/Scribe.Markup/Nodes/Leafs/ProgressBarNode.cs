namespace Scribe.Markup.Nodes.Leafs;

public class ProgressBarNode(int maxLength, int length) : ILeafNode
{
    public int MaxLength { get; } = maxLength < 0 ? 0 : maxLength;

    public int Length { get; } = length > maxLength ? maxLength : length; 
}
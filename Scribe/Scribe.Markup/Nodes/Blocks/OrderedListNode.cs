﻿namespace Scribe.Markup.Nodes.Blocks;

public class OrderedListNode(int listNumber) : IBlockNode
{
    public int ListNumber { get; } = listNumber;
    
    public List<IMarkupNode> Children { get; } = [];
}
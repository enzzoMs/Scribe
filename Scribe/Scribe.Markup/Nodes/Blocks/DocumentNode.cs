﻿namespace Scribe.Markup.Nodes.Blocks;

public class DocumentNode : IBlockNode
{
    public List<IMarkupNode> Children { get; } = [];
}
﻿namespace Scribe.Markup.Nodes.Leafs;

public class DividerNode(int length) : ILeafNode
{
    public const int MaxLength = 4;

    public int Length { get; } = length < 0 ? 0 : length > MaxLength ? MaxLength : length;
}
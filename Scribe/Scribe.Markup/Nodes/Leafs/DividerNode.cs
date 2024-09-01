﻿namespace Scribe.Markup.Nodes.Leafs;

public class DividerNode(int length) : IMarkupNode
{
    public const int MaxLength = 5;

    public int Length { get; } = length < 0 ? 0 : length > MaxLength ? MaxLength : length;
}
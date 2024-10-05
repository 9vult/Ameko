﻿// SPDX-License-Identifier: MPL-2.0

namespace AssCS.Overrides.Blocks;

/// <summary>
/// Any individual segment of an Event
/// </summary>
/// <remarks>
/// <para>
/// An event is made up of one or more Blocks.
/// Consider: <c>Hello {\i0}there{\i0} friend.</c>
/// </para>
/// <para>
/// That event would consist of the following blocks:
/// <list type="number">
/// <item>Plain text: <c>Hello </c></item>
/// <item>Override: <c>{\i1}</c></item>
/// <item>Plain text: <c>there</c></item>
/// <item>Override: <c>{\i0}</c></item>
/// <item>Plain text: <c> friend</c></item>
/// </list>
/// </para>
/// </remarks>
public abstract class Block
{
    protected string _text;
    private readonly BlockType _type;

    /// <summary>
    /// Type of block
    /// </summary>
    public BlockType Type => _type;

    /// <summary>
    /// Text content
    /// </summary>
    public virtual string Text => _text;

    /// <summary>
    /// Initialize a new block with a type
    /// </summary>
    /// <param name="data">Text data</param>
    /// <param name="type">Type of block</param>
    protected Block(string data, BlockType type)
    {
        _type = type;
        _text = data;
    }
}

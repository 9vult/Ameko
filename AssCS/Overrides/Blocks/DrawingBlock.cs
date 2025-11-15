// SPDX-License-Identifier: MPL-2.0

namespace AssCS.Overrides.Blocks;

/// <summary>
/// A drawing block
/// </summary>
/// <remarks>
/// Text following any drawing level greater than zero
/// is assumed to be a Drawing. A drawing is initiated by
/// a preceeding Override Block containing the following
/// override <c>P</c> tag:
/// <code>
/// {\p[drawinglevel]}
/// </code>
/// </remarks>
public class DrawingBlock(string data, int scale) : Block(data, BlockType.Drawing)
{
    /// <summary>
    /// Scale of the drawing
    /// </summary>
    public int Scale
    {
        get;
        set => field = value;
    } = scale;
}

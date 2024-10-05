// SPDX-License-Identifier: MPL-2.0

namespace AssCS.Overrides.Blocks;

/// <summary>
/// A plain-text block
/// </summary>
/// <remarks>
/// Any text in an event that is not a comment,
/// override, or drawing should be considered
/// to be in a Plain-Text block
/// </remarks>
public class PlainBlock(string data) : Block(data, BlockType.Plain) { }

// SPDX-License-Identifier: MPL-2.0

namespace Holo.Models;

/// <summary>
/// Defines a tab layout
/// </summary>
public class TabLayout
{
    /// <summary>
    /// Name of the layout
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Name of the person who designed the layout
    /// </summary>
    public required string Author { get; init; }

    /// <summary>
    /// Comma-seperated list of columns in the layout
    /// </summary>
    /// <remarks>
    /// Uses <c>*</c> for relative, <c>Auto</c> for auto, and numbers for exact
    /// </remarks>
    /// <example>
    /// Two equal-sized columns with a 4-px wide column in the middle for a splitter:
    /// <code>*, 4, *</code>
    /// </example>
    public required string ColumnDefinitions { get; init; }

    /// <summary>
    /// Comma-seperated list of rows in the layout
    /// </summary>
    /// <remarks>
    /// Uses <c>*</c> for relative, <c>Auto</c> for auto, and numbers for exact
    /// </remarks>
    /// <example>
    /// A half-size relative row and two full-size relative rows, intermixed by splitters:
    /// <code>0.5*, 4, *, 4, *</code>
    /// </example>
    public required string RowDefinitions { get; init; }

    /// <summary>
    /// Video section
    /// </summary>
    public required Section Video { get; init; }

    /// <summary>
    /// Audio section
    /// </summary>
    public required Section Audio { get; init; }

    /// <summary>
    /// Editing section
    /// </summary>
    public required Section Editor { get; init; }

    /// <summary>
    /// Events grid section
    /// </summary>
    public required Section Events { get; init; }
    public required Splitter[] Splitters { get; init; }
}

/// <summary>
/// A section in the tab
/// </summary>
public class Section
{
    /// <summary>
    /// If this section is visible
    /// </summary>
    public required bool IsVisible { get; init; }

    /// <summary>
    /// Column the section starts at
    /// </summary>
    /// <remarks>Zero-indexed</remarks>
    public required int Column { get; init; }

    /// <summary>
    /// Row the section starts at
    /// </summary>
    /// <remarks>Zero-indexed</remarks>
    public required int Row { get; init; }

    /// <summary>
    /// Number of columns the section spans
    /// </summary>
    /// <remarks>One-indexed</remarks>
    public int ColumnSpan { get; init; } = 1;

    /// <summary>
    /// Number of rows the section spans
    /// </summary>
    /// <remarks>One-indexed</remarks>
    public int RowSpan { get; init; } = 1;
}

public class Splitter
{
    /// <summary>
    /// Whether the splitter is vertical or horizontal
    /// </summary>
    public required bool IsVertical { get; init; }

    /// <summary>
    /// Column the section splitter at
    /// </summary>
    /// <remarks>Zero-indexed</remarks>
    public required int Column { get; init; }

    /// <summary>
    /// Row the splitter starts at
    /// </summary>
    /// <remarks>Zero-indexed</remarks>
    public required int Row { get; init; }

    /// <summary>
    /// Number of columns the splitter spans
    /// </summary>
    /// <remarks>One-indexed</remarks>
    public int ColumnSpan { get; init; } = 1;

    /// <summary>
    /// Number of rows the splitter spans
    /// </summary>
    /// <remarks>One-indexed</remarks>
    public int RowSpan { get; init; } = 1;
}

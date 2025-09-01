// SPDX-License-Identifier: MPL-2.0

namespace Holo.Models;

/// <summary>
/// Model for an item in a <see cref="Project"/>
/// </summary>
public class ProjectItemModel
{
    /// <summary>
    /// Optional display name override for the item
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// Type of item
    /// </summary>
    public ProjectItemType Type { get; init; }

    /// <summary>
    /// Relative path to the document
    /// </summary>
    /// <remarks><see langword="null"/> if the item is not a Document</remarks>
    public string? RelativePath { get; init; }

    /// <summary>
    /// Children of this item.
    /// </summary>
    /// <remarks>Empty if the item is not a Directory</remarks>
    public ProjectItemModel[] Children { get; init; } = [];
}

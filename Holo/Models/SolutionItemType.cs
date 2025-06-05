// SPDX-License-Identifier: MPL-2.0

namespace Holo.Models;

/// <summary>
/// Represents different types of <see cref="SolutionItem"/>s
/// </summary>
public enum SolutionItemType
{
    /// <summary>
    /// The <see cref="SolutionItem"/> is a <see cref="Document"/>
    /// </summary>
    Document = 0,

    /// <summary>
    /// The <see cref="SolutionItem"/> is a virtual directory
    /// </summary>
    Directory = 1,

    /// <summary>
    /// Unknown type
    /// </summary>
    Unknown = 2,
}

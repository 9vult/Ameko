// SPDX-License-Identifier: MPL-2.0

namespace Holo.Models;

/// <summary>
/// Represents different types of <see cref="ProjectItem"/>s
/// </summary>
public enum ProjectItemType
{
    /// <summary>
    /// The <see cref="ProjectItem"/> is a <see cref="Document"/>
    /// </summary>
    Document = 0,

    /// <summary>
    /// The <see cref="ProjectItem"/> is a virtual directory
    /// </summary>
    Directory = 1,

    /// <summary>
    /// Unknown type
    /// </summary>
    Unknown = 2,
}

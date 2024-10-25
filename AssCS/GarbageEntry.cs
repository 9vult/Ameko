// SPDX-License-Identifier: MPL-2.0

namespace AssCS;

/// <summary>
/// An Aegisub Project Garbage entry
/// </summary>
/// <param name="name">Name of the entry</param>
/// <param name="value">Value of the entry</param>
public struct GarbageEntry(string name, string value)
{
    /// <summary>
    /// Name to access the entry by
    /// </summary>
    public string Name = name;

    /// <summary>
    /// The value of the entry
    /// </summary>
    public string Value = value;
}

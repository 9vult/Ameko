// SPDX-License-Identifier: MPL-2.0

namespace AssCS;

/// <summary>
/// A single Advanced Substation Alpha subtitle file
/// </summary>
public class Document { }

/// <summary>
/// ASS file version
/// </summary>
public enum AssVersion
{
    /// <summary>
    /// v4.00
    /// </summary>
    V400 = 0,

    /// <summary>
    /// v4.00+
    /// </summary>
    V400P = 1,

    /// <summary>
    /// v4.00++
    /// </summary>
    V400PP = 2,

    /// <summary>
    /// Unknown
    /// </summary>
    UNKNOWN = -1,
}

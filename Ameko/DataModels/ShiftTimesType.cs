// SPDX-License-Identifier: GPL-3.0-only

namespace Ameko.DataModels;

/// <summary>
/// Type of shifting
/// </summary>
public enum ShiftTimesType
{
    /// <summary>
    /// Shift times by timestamp
    /// </summary>
    Time,

    /// <summary>
    /// Shift times by frame count
    /// </summary>
    Frames,

    /// <summary>
    /// Shift times by milliseconds
    /// </summary>
    Milliseconds,
}

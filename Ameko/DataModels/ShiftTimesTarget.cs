// SPDX-License-Identifier: GPL-3.0-only

namespace Ameko.DataModels;

/// <summary>
/// Which times should be shifted
/// </summary>
public enum ShiftTimesTarget
{
    /// <summary>
    /// Both start and end times
    /// </summary>
    Both,

    /// <summary>
    /// Start time
    /// </summary>
    Start,

    /// <summary>
    /// End time
    /// </summary>
    End,
}

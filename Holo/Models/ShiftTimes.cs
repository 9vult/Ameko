// SPDX-License-Identifier: MPL-2.0

namespace Holo.Models;

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
}

/// <summary>
/// Direction to shift times in
/// </summary>
public enum ShiftTimesDirection
{
    /// <summary>
    /// Shift times forward
    /// </summary>
    Forward,

    /// <summary>
    /// Shift times backward
    /// </summary>
    Backward,
}

/// <summary>
/// Which events should be shifted
/// </summary>
public enum ShiftTimesFilter
{
    /// <summary>
    /// All events in the document
    /// </summary>
    AllEvents,

    /// <summary>
    /// Only the currently-selected events
    /// </summary>
    SelectedEvents,
}

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

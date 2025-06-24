// SPDX-License-Identifier: GPL-3.0-only

namespace Ameko.DataModels;

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

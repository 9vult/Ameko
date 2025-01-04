// SPDX-License-Identifier: MPL-2.0

namespace AssCS.History;

/// <summary>
/// Type of change made in a commit
/// </summary>
[Flags]
public enum CommitType
{
    /// <summary>
    /// Potentially the entire file has been changed,
    /// any saved information should be discarded
    /// </summary>
    New = 0,

    /// <summary>
    /// The order of events in the file have changed
    /// </summary>
    Order = 1 << 0,

    /// <summary>
    /// Script info has been changed
    /// </summary>
    ScriptInfo = 1 << 1,

    /// <summary>
    /// A style has been changed
    /// </summary>
    Style = 1 << 2,

    /// <summary>
    /// Attachments have been changed
    /// </summary>
    Attachment = 1 << 3,

    /// <summary>
    /// Events have been added
    /// </summary>
    EventAdd = 1 << 4,

    /// <summary>
    /// Events have been removed
    /// </summary>
    /// <remarks>
    /// If the active event was removed, the active
    /// line should be updated before committing this change
    /// </remarks>
    EventRemove = 1 << 5,

    /// <summary>
    /// Metadata fields have been changed
    /// </summary>
    EventMeta = 1 << 6,

    /// <summary>
    /// The start and/or end times have been changed
    /// </summary>
    EventTime = 1 << 7,

    /// <summary>
    /// The text has been changed
    /// </summary>
    EventText = 1 << 8,

    /// <summary>
    /// Everything changed!
    /// </summary>
    EventFull = EventMeta | EventTime | EventText,

    /// <summary>
    /// Extradata entries were changed
    /// </summary>
    Extradata = 1 << 9,

    /// <summary>
    /// A style has been added
    /// </summary>
    StyleAdd = 1 << 10,

    /// <summary>
    /// A style has been removed
    /// </summary>
    StyleRemove = 1 << 11,
}

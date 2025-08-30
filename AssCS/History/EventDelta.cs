// SPDX-License-Identifier: MPL-2.0

namespace AssCS.History;

public class EventDelta(ChangeType type, Event? oldEvent, Event? newEvent, int? parentId = null)
{
    /// <summary>
    /// Type of change represented by this delta
    /// </summary>
    public ChangeType Type { get; } = type;

    /// <summary>
    /// State of the event prior to the change
    /// </summary>
    public Event? OldEvent { get; } = oldEvent?.Clone();

    /// <summary>
    /// State of the event after the change
    /// </summary>
    public Event? NewEvent { get; } = newEvent?.Clone();

    /// <summary>
    /// ID of the preceding event in the document (<see langword="null"/>) if first)
    /// </summary>
    public int? ParentId { get; } = parentId;
}

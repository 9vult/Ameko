// SPDX-License-Identifier: MPL-2.0

namespace AssCS.History;

/// <summary>
/// New state for an event
/// </summary>
/// <param name="type">Type of change</param>
/// <param name="target">Target event</param>
public class EventDelta(ChangeType type, Event? target)
{
    /// <summary>
    /// Type of change
    /// </summary>
    public ChangeType Type { get; } = type;

    /// <summary>
    /// State of the event after the change
    /// </summary>
    public Event? Target { get; } = target?.Clone();
}

/// <summary>
/// New state for a style
/// </summary>
/// <param name="type">Type of change</param>
/// <param name="target">Target style</param>
public class StyleDelta(ChangeType type, Style? target)
{
    /// <summary>
    /// Type of change
    /// </summary>
    public ChangeType Type { get; } = type;

    /// <summary>
    /// State of the style after the change
    /// </summary>
    public Style? Target { get; } = target?.Clone();
}

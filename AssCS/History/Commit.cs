// SPDX-License-Identifier: MPL-2.0

namespace AssCS.History;

/// <summary>
/// A commit in history
/// </summary>
public interface ICommit
{
    public int Id { get; }
}

/// <summary>
/// A commit for changes to events
/// </summary>
public class EventCommit(int id) : ICommit
{
    public int Id { get; } = id;
    public List<EventDelta> Deltas { get; init; } = [];
}

/// <summary>
/// A commit for changes to styles
/// </summary>
public class StyleCommit : ICommit
{
    public required int Id { get; set; }
    public required Style Target { get; set; }
    public required ChangeType Type { get; set; }
}

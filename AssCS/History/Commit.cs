// SPDX-License-Identifier: MPL-2.0

namespace AssCS.History;

/// <summary>
/// A commit in history
/// </summary>
public interface ICommit
{
    public int Id { get; }
    public string Message { get; }
}

/// <summary>
/// A commit for changes to events
/// </summary>
public class EventCommit : ICommit
{
    public required int Id { get; set; }
    public required string Message { get; set; }
    public required List<EventLink> Targets { get; set; }
}

/// <summary>
/// A commit for changes to styles
/// </summary>
public class StyleCommit : ICommit
{
    public required int Id { get; set; }
    public required string Message { get; set; }
    public required Style Target { get; set; }
    public required CommitType Type { get; set; }
}

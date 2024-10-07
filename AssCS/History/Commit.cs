// SPDX-License-Identifier: MPL-2.0

namespace AssCS.History;

/// <summary>
/// A commit in history
/// </summary>
public interface ICommit;

/// <summary>
/// A commit for changes to events
/// </summary>
public class EventCommit : ICommit
{
    public required int Id;
    public required string Message;
    public required List<EventLink> Targets;
}

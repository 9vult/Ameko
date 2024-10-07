// SPDX-License-Identifier: MPL-2.0

namespace AssCS.History;

/// <summary>
/// Links an event to its parent
/// </summary>
/// <remarks>
/// When committing to history, we want to keep track of where the event was
/// positioned in the file so we can place it in the correct position in
/// case of an undo.
/// </remarks>
public struct EventLink
{
    private Event _target;

    /// <summary>
    /// ID of the parent event, or <see langword="null"/> if there is no parent
    /// </summary>
    /// <remarks>
    /// The parent event is the event preceeding the target event in the file.
    /// If the target event is the first event in the file, then the parent
    /// is <see langword="null"/>.
    /// </remarks>
    public int? ParentId;

    /// <summary>
    /// The target event
    /// </summary>
    /// <remarks>
    /// This accessor automatically runs <see cref="Event.Clone"/>
    /// on get and set to prevent pass-by-reference issues.
    /// </remarks>
    public Event Target
    {
        readonly get => _target.Clone();
        set => _target = value.Clone();
    }

    /// <summary>
    /// The action performed
    /// </summary>
    public CommitType Type;
}

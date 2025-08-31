// SPDX-License-Identifier: MPL-2.0

using System.Collections.Concurrent;

namespace AssCS.History;

/// <summary>
/// Manage history operations for a document
/// </summary>
public class HistoryManager : BindableBase
{
    private int _nextId;
    private readonly ConcurrentStack<ICommit> _history = [];
    private readonly ConcurrentStack<ICommit> _future = [];

    private readonly List<Event> _initialState = [];
    private Style? _initialStyleState = null;

    /// <summary>
    /// Next commit ID
    /// </summary>
    private int NextId => _nextId++;

    /// <summary>
    /// If there is history to undo
    /// </summary>
    public bool CanUndo => _history.Count > 0;

    /// <summary>
    /// If there is future to redo
    /// </summary>
    public bool CanRedo => _future.Count > 0;

    /// <summary>
    /// The ID of the most recent commit, used for coalescing
    /// </summary>
    public int LastId { get; private set; } = -1;

    /// <summary>
    /// The type of the most recent commit, used for coalescing
    /// </summary>
    public ChangeType LastCommitType { get; private set; }
    public DateTimeOffset LastCommitTime { get; private set; }

    /// <summary>
    /// Signal that modifications may be starting
    /// </summary>
    /// <param name="events">List of events to track</param>
    public void BeginTransaction(IEnumerable<Event> events)
    {
        _initialState.Clear();
        _initialState.AddRange(events.Select(e => e.Clone()));
    }

    /// <summary>
    /// Signal that modifications may be starting
    /// </summary>
    /// <param name="event">Event to track</param>
    public void BeginTransaction(Event @event)
    {
        _initialState.Clear();
        _initialState.Add(@event.Clone());
    }

    public void BeginTransaction(Style style)
    {
        _initialStyleState = style;
    }

    /// <summary>
    /// Commit an event change to history
    /// </summary>
    /// <remarks>
    /// Side effect: Clears future stack
    /// </remarks>
    /// <param name="type">Type of change</param>
    /// <param name="event">Target line</param>
    /// <param name="parentId">ID of the parent of the target</param>
    /// <param name="coalesce">Whether to amend the previous commit or not</param>
    /// <returns>ID of the commit</returns>
    /// <exception cref="InvalidOperationException">If an amend is attempted to a non-Event commit</exception>
    public int Commit(ChangeType type, Event @event, int? parentId, bool coalesce = false)
    {
        EventCommit commit;
        if (coalesce)
        {
            if (!CanUndo)
                throw new InvalidOperationException("Cannot amend, no commits in the undo stack!");
            if (!_history.TryPeek(out var latest) || latest is not EventCommit eventCommit)
                throw new InvalidOperationException("Cannot amend to a non-Event commit");
            commit = eventCommit;
        }
        else
            commit = new EventCommit(NextId);

        switch (type)
        {
            case ChangeType.Add:
                commit.Deltas.Add(new EventDelta(type, null, @event, parentId));
                break;
            case ChangeType.Remove:
                commit.Deltas.Add(new EventDelta(type, @event, null, parentId));
                break;
            case ChangeType.Modify:
                commit.Deltas.Add(
                    new EventDelta(
                        type,
                        _initialState.FirstOrDefault(e => e.Id == @event.Id),
                        @event,
                        parentId
                    )
                );
                break;
        }
        if (!coalesce)
            _history.Push(commit);

        LastCommitTime = DateTimeOffset.Now;
        LastCommitType = type;
        LastId = commit.Id;
        _future.Clear();
        NotifyAbilitiesChanged();
        return commit.Id;
    }

    /// <summary>
    /// Commit a style change to history
    /// </summary>
    /// <remarks>
    /// Side effect: Clears future stack
    /// </remarks>
    /// <param name="type">Type of change</param>
    /// <param name="target">Target style</param>
    /// <returns>ID of the commit</returns>
    public int Commit(ChangeType type, Style target)
    {
        var id = NextId;
        _history.Push(
            new StyleCommit
            {
                Id = id,
                Target = target,
                Type = type,
            }
        );

        LastCommitTime = DateTimeOffset.Now;
        LastCommitType = type;
        _future.Clear();
        NotifyAbilitiesChanged();
        return id;
    }

    /// <summary>
    /// Retrieve the latest commit from the Undo stack
    /// and add it to the Redo stack
    /// </summary>
    /// <remarks>
    /// It is the duty of the calling member to act on the commit.
    /// </remarks>
    /// <returns>The commit</returns>
    /// <exception cref="InvalidOperationException">If the Undo stack is empty</exception>
    public ICommit Undo()
    {
        if (!CanUndo || !_history.TryPop(out var commit))
            throw new InvalidOperationException("Cannot undo, no commits in the undo stack!");

        _future.Push(commit);

        LastCommitTime = DateTimeOffset.Now;
        LastCommitType = ChangeType.TimeMachine;
        NotifyAbilitiesChanged();
        return commit;
    }

    /// <summary>
    /// Retrieve the latest commit from the Redo stack
    /// and add it to the Undo stack
    /// </summary>
    /// <remarks>
    /// It is the duty of the calling member to act on the commit.
    /// </remarks>
    /// <returns>The commit</returns>
    /// <exception cref="InvalidOperationException">If the Redo stack is empty</exception>
    public ICommit Redo()
    {
        if (!CanRedo || !_future.TryPop(out var commit))
            throw new InvalidOperationException("Cannot redo, no commits in the redo stack!");

        _history.Push(commit);

        LastCommitTime = DateTimeOffset.Now;
        LastCommitType = ChangeType.TimeMachine;
        NotifyAbilitiesChanged();
        return commit;
    }

    public ICommit PeekHistory()
    {
        if (!CanUndo || !_history.TryPeek(out var commit))
            throw new InvalidOperationException(
                "Cannot peek history, no commits in the undo stack!"
            );
        return commit;
    }

    public ICommit PeekFuture()
    {
        if (!CanRedo || !_future.TryPeek(out var commit))
            throw new InvalidOperationException(
                "Cannot peek future, no commits in the redo stack!"
            );
        return commit;
    }

    /// <summary>
    /// Raise property changed events for undo/redo abilities.
    /// </summary>
    private void NotifyAbilitiesChanged()
    {
        RaisePropertyChanged(nameof(CanUndo));
        RaisePropertyChanged(nameof(CanRedo));
        OnChangeMade?.Invoke(this, EventArgs.Empty);
    }

    public event EventHandler<EventArgs>? OnChangeMade;
}

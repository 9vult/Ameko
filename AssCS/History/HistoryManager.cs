// SPDX-License-Identifier: MPL-2.0

using System.Collections.Concurrent;
using System.Collections.ObjectModel;

namespace AssCS.History;

/// <summary>
/// Manage history operations for a document
/// </summary>
public class HistoryManager(
    EventManager eventManager,
    StyleManager styleManager,
    ExtradataManager extradataManager
) : BindableBase
{
    private readonly ConcurrentStack<Commit> _history = [];
    private readonly ConcurrentStack<Commit> _future = [];

    private readonly List<Event> _initialState = [];
    private Style? _initialStyleState = null;

    /// <summary>
    /// Next commit ID
    /// </summary>
    private int NextId => field++;

    /// <summary>
    /// If there is history to undo
    /// </summary>
    public bool CanUndo => _history.Count >= 2;

    /// <summary>
    /// If there is future to redo
    /// </summary>
    public bool CanRedo => !_future.IsEmpty;

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
    /// Get the initial event state, useful for performing diff checks
    /// </summary>
    public ReadOnlyCollection<Event> InitialState => _initialState.AsReadOnly();

    /// <summary>
    /// Commit the current state to history
    /// </summary>
    /// <remarks>
    /// Side effect: Clears future stack
    /// </remarks>
    /// <returns>ID of the commit</returns>
    public int Commit(ChangeType type)
    {
        var commit = CreateCommit(type);
        _history.Push(commit);

        LastCommitTime = DateTimeOffset.Now;
        LastCommitType = ChangeType.ModifyEvent;
        LastId = commit.Id;
        _future.Clear();
        NotifyAbilitiesChanged();
        return commit.Id;
    }

    /// <summary>
    /// Commit an event modification to history
    /// </summary>
    /// <remarks>
    /// Side effect: Clears future stack
    /// </remarks>
    /// <param name="target">The new state of the modified event</param>
    /// <param name="coalesce">Whether to amend the previous commit or not</param>
    /// <returns>ID of the commit</returns>
    /// <exception cref="InvalidOperationException">If an amend is attempted where disallowed</exception>
    public int Commit(Event target, bool coalesce = false)
    {
        Commit commit;
        if (coalesce)
        {
            if (!CanUndo || !_history.TryPeek(out var latest))
                throw new InvalidOperationException("Cannot amend, no commits in the undo stack!");
            if (latest.Type != ChangeType.ModifyEvent)
                throw new InvalidOperationException("Cannot amend to a different change type!");
            commit = latest;
        }
        else
            commit = CreateCommit(ChangeType.ModifyEvent);

        // Perform the coalescing
        if (coalesce && commit.Events.TryGetValue(target.Id, out var modEvent))
        {
            modEvent.SetFields(EventField.All, target);
        }
        else
            _history.Push(commit);

        LastCommitTime = DateTimeOffset.Now;
        LastCommitType = ChangeType.ModifyEvent;
        LastId = commit.Id;
        _future.Clear();
        NotifyAbilitiesChanged();
        return commit.Id;
    }

    /// <summary>
    /// Commit a style modification to history
    /// </summary>
    /// <remarks>
    /// Side effect: Clears future stack
    /// </remarks>
    /// <param name="target">The new state of the modified style</param>
    /// <param name="coalesce">Whether to amend the previous commit or not</param>
    /// <returns>ID of the commit</returns>
    /// <exception cref="InvalidOperationException">If an amend is attempted where disallowed</exception>
    public int Commit(Style target, bool coalesce = false)
    {
        Commit commit;
        if (coalesce)
        {
            if (!CanUndo || !_history.TryPeek(out var latest))
                throw new InvalidOperationException("Cannot amend, no commits in the undo stack!");
            if (latest.Type != ChangeType.ModifyStyle)
                throw new InvalidOperationException("Cannot amend to a different change type!");
            commit = latest;
        }
        else
            commit = CreateCommit(ChangeType.ModifyStyle);

        // Perform the coalescing
        if (coalesce && commit.Styles.Any(s => s.Id == target.Id))
        {
            var modStyle = commit.Styles.First(s => s.Id == target.Id);
            modStyle.SetFields(StyleField.All, target);
        }
        else
            _history.Push(commit);

        LastCommitTime = DateTimeOffset.Now;
        LastCommitType = ChangeType.ModifyStyle;
        LastId = commit.Id;
        _future.Clear();
        NotifyAbilitiesChanged();
        return commit.Id;
    }

    /// <summary>
    /// Retrieve the latest commit from the Undo stack,
    /// apply its state, and add it to the Redo stack
    /// </summary>
    /// <returns><see langword="true"/> if the operation was successful</returns>
    public bool Undo()
    {
        if (!CanUndo || !_history.TryPop(out var top))
            return false;

        _future.Push(top);
        if (!_history.TryPeek(out var commit))
            throw new InvalidOperationException("Undo failed, no commits in the undo stack!");

        eventManager.RestoreState(commit);
        styleManager.RestoreState(commit);
        extradataManager.RestoreState(commit);

        LastCommitTime = DateTimeOffset.Now;
        LastCommitType = ChangeType.TimeMachine;
        NotifyAbilitiesChanged();
        return true;
    }

    /// <summary>
    /// Retrieve the latest commit from the Redo stack,
    /// apply its state, and add it to the Undo stack
    /// </summary>
    /// <returns><see langword="true"/> if the operation was successful</returns>
    public bool Redo()
    {
        if (!CanRedo || !_future.TryPop(out var commit))
            return false;

        _history.Push(commit);

        eventManager.RestoreState(commit);
        styleManager.RestoreState(commit);
        extradataManager.RestoreState(commit);

        LastCommitTime = DateTimeOffset.Now;
        LastCommitType = ChangeType.TimeMachine;
        NotifyAbilitiesChanged();
        return true;
    }

    public Commit PeekHistory()
    {
        if (_history.IsEmpty || !_history.TryPeek(out var commit))
            throw new InvalidOperationException(
                "Cannot peek history, no commits in the undo stack!"
            );
        return commit;
    }

    public Commit PeekFuture()
    {
        if (_future.IsEmpty || !_future.TryPeek(out var commit))
            throw new InvalidOperationException(
                "Cannot peek future, no commits in the redo stack!"
            );
        return commit;
    }

    /// <summary>
    /// Create a commit with the current state
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    private Commit CreateCommit(ChangeType type)
    {
        var (chain, events) = eventManager.GetState();

        return new Commit(
            NextId,
            type,
            chain,
            events,
            styleManager.GetState(),
            extradataManager.Get().Select(e => e.Clone()).ToList()
        );
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

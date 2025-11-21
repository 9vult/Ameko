// SPDX-License-Identifier: MPL-2.0

using System.Collections.Concurrent;

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
    /// The type of the most recent commit, used for coalescing
    /// </summary>
    public ChangeType LastCommitType { get; private set; }
    public DateTimeOffset LastCommitTime { get; private set; }

    /// <summary>
    /// Commit the current state to history
    /// </summary>
    /// <remarks>
    /// Side effect: Clears future stack
    /// </remarks>
    /// <param name="type">Change type</param>
    /// <param name="selection">Selected events</param>
    /// <returns>ID of the commit</returns>
    public int Commit(ChangeType type, IReadOnlyList<Event>? selection = null)
    {
        var commit = CreateCommit(type, selection);
        _history.Push(commit);

        LastCommitTime = DateTimeOffset.Now;
        LastCommitType = type;
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
    /// <param name="type">Change type</param>
    /// <param name="target">The new state of the modified event</param>
    /// <param name="coalesce">Whether to amend the previous commit or not</param>
    /// <param name="selection">Selected events</param>
    /// <returns>ID of the commit</returns>
    /// <exception cref="InvalidOperationException">If an amend is attempted where disallowed</exception>
    public int Commit(
        ChangeType type,
        Event target,
        bool coalesce = false,
        IReadOnlyList<Event>? selection = null
    )
    {
        Commit commit;
        if (coalesce)
        {
            if (!CanUndo || !_history.TryPeek(out var latest))
                throw new InvalidOperationException("Cannot amend, no commits in the undo stack!");
            if (type != ChangeType.ModifyEventText && type != ChangeType.ModifyEventMeta)
                throw new InvalidOperationException("Cannot amend non-Modify changes!");
            if (latest.Type != type)
                throw new InvalidOperationException("Cannot amend to a different change type!");
            commit = latest;
        }
        else
            commit = CreateCommit(type, selection);

        // Perform the coalescing
        if (coalesce && commit.Events.TryGetValue(target.Id, out var modEvent))
        {
            modEvent.SetFields(EventField.All, target);
        }
        else
            _history.Push(commit);

        LastCommitTime = DateTimeOffset.Now;
        LastCommitType = type;
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
            commit = CreateCommit(ChangeType.ModifyStyle, null);

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
        _future.Clear();
        NotifyAbilitiesChanged();
        return commit.Id;
    }

    /// <summary>
    /// Retrieve the latest commit from the Undo stack,
    /// apply its state, and add it to the Redo stack
    /// </summary>
    /// <returns>The commit, or <see langword="null"/> on failure</returns>
    public Commit? Undo()
    {
        if (!CanUndo || !_history.TryPop(out var top))
            return null;

        _future.Push(top);
        if (!_history.TryPeek(out var commit))
            throw new InvalidOperationException("Undo failed, no commits in the undo stack!");

        eventManager.RestoreState(commit);
        styleManager.RestoreState(commit);
        extradataManager.RestoreState(commit);

        LastCommitTime = DateTimeOffset.Now;
        LastCommitType = ChangeType.TimeMachine;
        NotifyAbilitiesChanged();
        return commit;
    }

    /// <summary>
    /// Retrieve the latest commit from the Redo stack,
    /// apply its state, and add it to the Undo stack
    /// </summary>
    /// <returns>The commit, or <see langword="null"/> on failure</returns>
    public Commit? Redo()
    {
        if (!CanRedo || !_future.TryPop(out var commit))
            return null;

        _history.Push(commit);

        eventManager.RestoreState(commit);
        styleManager.RestoreState(commit);
        extradataManager.RestoreState(commit);

        LastCommitTime = DateTimeOffset.Now;
        LastCommitType = ChangeType.TimeMachine;
        NotifyAbilitiesChanged();
        return commit;
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
    /// <param name="selection">Selected events</param>
    /// <returns>Commit containing the current state</returns>
    private Commit CreateCommit(ChangeType type, IReadOnlyList<Event>? selection)
    {
        var (chain, events) = eventManager.GetState();

        return new Commit(
            Id: NextId,
            Type: type,
            Chain: chain,
            Events: events,
            Styles: styleManager.GetState(),
            Extradata: extradataManager.Get().Select(e => e.Clone()).ToList(),
            Selection: selection
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

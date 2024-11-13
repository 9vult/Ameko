// SPDX-License-Identifier: MPL-2.0

namespace AssCS.History;

/// <summary>
/// Manage history operations for a document
/// </summary>
public class HistoryManager : BindableBase
{
    private int _nextId = 0;
    private readonly Stack<ICommit> _history = [];
    private readonly Stack<ICommit> _future = [];

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
    /// Commit an event change to history
    /// </summary>
    /// <remarks>
    /// Side-effect: Clears future stack
    /// </remarks>
    /// <param name="description">Short description of the change</param>
    /// <param name="type">Type of change</param>
    /// <param name="target">Target line</param>
    /// <param name="parentId">ID of the parent of the target</param>
    /// <param name="amend">Whether to amend the previous commit or not</param>
    /// <returns>ID of the commit</returns>
    /// <exception cref="InvalidOperationException">If an amend is attempted to a non-Event commit</exception>
    public int Commit(
        string description,
        CommitType type,
        ref Event target,
        int? parentId,
        bool amend = false
    )
    {
        if (!amend)
        {
            var id = NextId;
            _history.Push(
                new EventCommit
                {
                    Id = id,
                    Message = description,
                    Targets =
                    [
                        new EventLink
                        {
                            ParentId = parentId,
                            Target = target,
                            Type = type,
                        },
                    ],
                }
            );
            _future.Clear();
            NotifyAbilitiesChanged();
            return id;
        }

        if (!CanUndo)
            throw new InvalidOperationException("Cannot amend, no commits in the undo stack!");

        if (_history.Peek() is EventCommit commit)
        {
            commit.Message += ";" + description;
            commit.Targets.Add(
                new EventLink
                {
                    ParentId = parentId,
                    Target = target,
                    Type = type,
                }
            );
            _future.Clear();
            NotifyAbilitiesChanged();
            return commit.Id;
        }

        throw new InvalidOperationException("Cannot amend an Event commit to a non-Event commit");
    }

    /// <summary>
    /// Commit a style change to history
    /// </summary>
    /// <remarks>
    /// Side-effect: Clears future stack
    /// </remarks>
    /// <param name="description"></param>
    /// <param name="type">Type of change</param>
    /// <param name="target">Target style</param>
    /// <returns>ID of the commit</returns>
    public int Commit(string description, CommitType type, ref Style target)
    {
        var id = NextId;
        _history.Push(
            new StyleCommit
            {
                Id = id,
                Message = description,
                Target = target,
                Type = type,
            }
        );
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
        if (!CanUndo)
            throw new InvalidOperationException("Cannot undo, no commits in the undo stack!");
        var commit = _history.Pop();
        _future.Push(commit);
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
        if (!CanRedo)
            throw new InvalidOperationException("Cannot redo, no commits in the redo stack!");
        var commit = _future.Pop();
        _history.Push(commit);
        NotifyAbilitiesChanged();
        return commit;
    }

    /// <summary>
    /// Raise property changed events for undo/redo abilities
    /// </summary>
    private void NotifyAbilitiesChanged()
    {
        RaisePropertyChanged(nameof(CanUndo));
        RaisePropertyChanged(nameof(CanRedo));
    }
}

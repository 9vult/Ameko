﻿// SPDX-License-Identifier: MPL-2.0

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
    public CommitType LastCommitType { get; private set; }
    public DateTimeOffset LastCommitTime { get; private set; }

    /// <summary>
    /// Commit an event change to history
    /// </summary>
    /// <remarks>
    /// Side effect: Clears future stack
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
        Event target,
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

            LastCommitTime = DateTimeOffset.Now;
            LastCommitType = type;
            _future.Clear();
            NotifyAbilitiesChanged();

            return id;
        }

        if (!CanUndo)
            throw new InvalidOperationException("Cannot amend, no commits in the undo stack!");
        if (!_history.TryPeek(out var latest) || latest is not EventCommit commit)
            throw new InvalidOperationException(
                "Cannot amend an Event commit to a non-Event commit"
            );

        commit.Message += ";" + description;
        commit.Targets.Add(
            new EventLink
            {
                ParentId = parentId,
                Target = target,
                Type = type,
            }
        );

        LastCommitTime = DateTimeOffset.Now;
        LastCommitType = type;
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
    /// <param name="description"></param>
    /// <param name="type">Type of change</param>
    /// <param name="target">Target style</param>
    /// <returns>ID of the commit</returns>
    public int Commit(string description, CommitType type, Style target)
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
        LastCommitType = CommitType.TimeMachine;
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
        LastCommitType = CommitType.TimeMachine;
        NotifyAbilitiesChanged();
        return commit;
    }

    /// <summary>
    /// Raise property changed events for undo/redo abilities.
    /// </summary>
    private void NotifyAbilitiesChanged()
    {
        RaisePropertyChanged(nameof(CanUndo));
        RaisePropertyChanged(nameof(CanRedo));
    }
}

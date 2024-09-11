using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace AssCS.History
{
    public readonly struct Commit<T> where T : ICommitable
    {
        public ReadOnlyCollection<CommitAction<T>> Actions { get; }
        public CommitActionType Type { get; }

        public Commit(ReadOnlyCollection<CommitAction<T>> actions, CommitActionType type = CommitActionType.Edit)
        {
            Actions = actions;
            Type = type;
        }

        public Commit(List<CommitAction<T>> actions, CommitActionType type = CommitActionType.Edit)
            : this(actions.AsReadOnly(), type) { }
        public Commit(CommitAction<T> action, CommitActionType type = CommitActionType.Edit)
            : this(new ReadOnlyCollection<CommitAction<T>>(new[] { action }), type) { }
    }
}

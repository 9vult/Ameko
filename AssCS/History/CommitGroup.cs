using System.Collections.Generic;

namespace AssCS.History
{
    public readonly struct CommitGroup<T> where T : ICommitable
    {
        public IReadOnlyCollection<Commit<T>> Commits { get; }

        public CommitGroup(IReadOnlyCollection<Commit<T>> commits)
        {
            Commits = commits;
        }

        public CommitGroup(List<Commit<T>> commits)
        {
            Commits = commits.AsReadOnly();
        }

        public CommitGroup(Commit<T> commit)
        {
            Commits = new[] { commit };
        }
    }
}

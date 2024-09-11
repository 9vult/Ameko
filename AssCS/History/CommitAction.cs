namespace AssCS.History
{
    public readonly struct CommitAction<T> where T : ICommitable
    {
        public int? Parent { get; }
        public T Target { get; }

        public CommitAction(T target, int? parent)
        {
            Parent = parent;
            Target = target;
        }
    }
}

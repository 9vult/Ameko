using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AssCS.History
{
    public class HistoryManager : INotifyPropertyChanged
    {
        private readonly Stack<CommitGroup<Event>> _eventHistory;
        private readonly Stack<CommitGroup<Event>> _eventFuture;

        public bool EventCanGoBack => _eventHistory.Count > 0;
        public bool EventCanGoForward => _eventFuture.Count > 0;

        public CommitGroup<Event> EventGoBack()
        {
            var result = _eventHistory.Pop();
            Notify();
            return result;
        }

        public CommitGroup<Event> EventGoForward()
        {
            var result = _eventFuture.Pop();
            Notify();
            return result;
        }

        public void Commit(CommitGroup<Event> commit)
        {
            _eventHistory.Push(commit);
            _eventFuture.Clear();
            Notify();
        }

        public void PushHistory(CommitGroup<Event> commit)
        {
            _eventHistory.Push(commit);
            Notify();
        }

        public void PushFuture(CommitGroup<Event> commit)
        {
            _eventFuture.Push(commit);
            Notify();
        }

        public HistoryManager()
        {
            _eventHistory = new Stack<CommitGroup<Event>>();
            _eventFuture = new Stack<CommitGroup<Event>>();
        }

        private void Notify()
        {
            OnPropertyChanged(nameof(EventCanGoBack));
            OnPropertyChanged(nameof(EventCanGoForward));
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

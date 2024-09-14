using AssCS;
using AssCS.History;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Holo
{
    public class FileWrapper : INotifyPropertyChanged
    {
        private readonly File file;
        private Event? selectedEvent;
        private List<Event>? selectedEvents;
        private Uri? filePath;
        private bool upToDate;
        private string title;

        private Event? previouslySelectedEvent;
        private List<Event>? previouslySelectedEvents;

        private AVManager avManager;

        private readonly Logger logger;

        public File File => file;
        public int ID { get; }

        public AVManager AVManager => avManager;

        public List<Event>? SelectedEventCollection
        {
            get => selectedEvents;
            private set => selectedEvents = value;
        }

        public Event? SelectedEvent
        {
            get => selectedEvent;
            private set { selectedEvent = value; OnPropertyChanged(nameof(SelectedEvent)); }
        }

        public Uri? FilePath
        {
            get => filePath;
            set { filePath = value; OnPropertyChanged(nameof(FilePath)); }
        }

        public bool UpToDate
        {
            get => upToDate;
            set { upToDate = value; OnPropertyChanged(nameof(UpToDate)); }
        }

        public string Title
        {
            get => title;
            set { title = value; OnPropertyChanged(nameof(Title)); }
        }

        private readonly object _processingLock = new object();

        /// <summary>
        /// Handler for when the selected events change
        /// </summary>
        /// <param name="newSelectedEvents">List of the new selected event</param>
        /// <param name="newSelectedEvent">The currently selected event</param>
        public void SelectedEventsChanged(List<Event> newSelectedEvents, Event newSelectedEvent)
        {
            SelectedEventCollection = new List<Event>(newSelectedEvents);
            ProcessSelectionChange(newSelectedEvents, newSelectedEvent, shouldCommit: true);
        }

        /// <summary>
        /// Set the currently selected events
        /// </summary>
        /// <param name="newSelectedEvents">Events to select</param>
        /// <param name="newSelectedEvent">Event to select</param>
        /// <returns></returns>
        public Commit<Event>? SetSelectedEvents(List<Event> newSelectedEvents, Event newSelectedEvent)
        {
            var result = ProcessSelectionChange(newSelectedEvents, newSelectedEvent, shouldCommit: false);
            SelectedEvent = newSelectedEvent;
            SelectedEventCollection = new List<Event>(newSelectedEvents);
            return result;
        }

        /// <summary>
        /// Process changes for history
        /// </summary>
        /// <param name="newSelectedEvents">Selected events</param>
        /// <param name="newSelectedEvent">Currently selected event</param>
        /// <param name="shouldCommit">Should this function commit to history?</param>
        /// <returns>Commit object</returns>
        public Commit<Event>? ProcessSelectionChange(List<Event> newSelectedEvents, Event newSelectedEvent, bool shouldCommit)
        {
            lock (_processingLock)
            {
                if (previouslySelectedEvent is null || previouslySelectedEvents is null)
                {
                    // Nothing to check against, commit new events
                    logger.Debug("SELECT→ No Prior", "FileWrapper");
                    previouslySelectedEvent = newSelectedEvent.Clone();
                    previouslySelectedEvents = newSelectedEvents.Select(e => e.Clone()).ToList();

                    var commit = new Commit<Event>(
                        newSelectedEvents.Select(e => new CommitAction<Event>(e.Clone(), file.EventManager.GetBefore(e.Id)?.Id)).ToList(),
                        CommitActionType.Edit
                    );
                    if (shouldCommit)
                        file.HistoryManager.Commit(new CommitGroup<Event>(commit));
                    SelectedEvent = newSelectedEvent;
                    return commit;
                }

                if (newSelectedEvents.Count == 0)
                {
                    // Nothing selected, skip
                    logger.Debug("SELECT→ Empty (skipping)", "FileWrapper");
                    return null;
                }

                var has = file.EventManager.TryGet(previouslySelectedEvent.Id, out var target);
                if ((has && previouslySelectedEvent.Equals(target)) || !has)
                {
                    // Nothing changed, skip
                    logger.Debug("SELECT→ No change", "FileWrapper");
                    previouslySelectedEvent = newSelectedEvent.Clone();
                    previouslySelectedEvents = newSelectedEvents.Select(e => e.Clone()).ToList();
                    SelectedEvent = newSelectedEvent;
                    return null;
                }
                else
                {
                    // Something changed, commit changed events
                    logger.Debug("SELECT→ Committing changes", "FileWrapper");

                    var changedEvents = previouslySelectedEvents.Select(e => file.EventManager.Get(e.Id)).ToList();
                    var changedEvent = file.EventManager.Get(previouslySelectedEvent.Id);
                    PropagateChanges(changedEvents, previouslySelectedEvent, changedEvent);

                    var commit = new Commit<Event>(
                        changedEvents.Select(e => new CommitAction<Event>(
                            e.Clone(),
                            file.EventManager.GetBefore(e.Id)?.Id)
                        ).ToList()
                    );
                    if (shouldCommit)
                        file.HistoryManager.Commit(new CommitGroup<Event>(commit));
                    previouslySelectedEvent = newSelectedEvent.Clone();
                    previouslySelectedEvents = newSelectedEvents.Select(e => e.Clone()).ToList();
                    SelectedEvent = newSelectedEvent;
                    UpToDate = false;
                    return commit;
                }
            }
        }

        public Commit<Event> Remove(List<Event> selectedEvents, Event selectedEvent, bool shouldCommit = true)
        {
            // Save the parents
            var parents = selectedEvents.ToDictionary(e => e.Id, e => file.EventManager.GetBefore(e.Id)?.Id);

            if (selectedEvents.Count > 1)
            {
                // Remove all but the "currently selected" one
                foreach (var evnt in selectedEvents.Where(e => e.Id != selectedEvent.Id))
                {
                    file.EventManager.Remove(evnt.Id);
                }
            }
            // Commit the list, remove the final one, and select an adjacent event
            var want = SelectAdjOrDefault(selectedEvent);
            var commit = new Commit<Event>(
                selectedEvents.Select(e =>
                    new CommitAction<Event>(e.Clone(), parents[e.Id])).ToList(),
                CommitActionType.Delete);
            if (shouldCommit) file.HistoryManager.Commit(new CommitGroup<Event>(commit));
            file.EventManager.Remove(selectedEvent.Id);
            SelectedEventCollection = new List<Event> { want };
            SelectedEvent = want;
            UpToDate = false;
            return commit;
        }

        public Commit<Event> Add(List<Event> selectedEvents, Event selectedEvent, bool select, bool shouldCommit = true)
        {
            var commits = new List<Commit<Event>>();

            var commit = new Commit<Event>(
                selectedEvents.Select(e =>
                    new CommitAction<Event>(e.Clone(), file.EventManager.GetBefore(e.Id)?.Id)).ToList(),
                CommitActionType.Insert);
            commits.Add(commit);

            if (select)
            {
                var selectCommit = SetSelectedEvents(selectedEvents, selectedEvent);
                if (selectCommit != null) commits.Add((Commit<Event>)selectCommit);
            }

            if (shouldCommit) file.HistoryManager.Commit(new CommitGroup<Event>(commits));
            if (select)
            {
                SelectedEvent = selectedEvent;
                SelectedEventCollection = selectedEvents;
            }
            UpToDate = false;
            return commit;
        }

        public void Undo()
        {
            if (!file.HistoryManager.EventCanGoBack) return;
            var historyCommitGroup = file.HistoryManager.EventGoBack();

            var newFuture = new List<Commit<Event>>();

            foreach (var commit in historyCommitGroup.Commits)
            {
                switch (commit.Type)
                {
                    case CommitActionType.Edit:
                        {
                            var future = new List<CommitAction<Event>>();
                            future.AddRange(commit.Actions.Select(a => new CommitAction<Event>(file.EventManager.Get(a.Target.Id).Clone(), a.Parent)));
                            file.EventManager.ReplaceInplace(commit.Actions.Select(a => a.Target.Clone()));
                            newFuture.Add(new Commit<Event>(future));
                        }
                        break;
                    case CommitActionType.Delete:
                        {
                            var future = new List<CommitAction<Event>>();
                            foreach (var action in commit.Actions)
                            {
                                future.Add(new CommitAction<Event>(action.Target.Clone(), action.Parent));
                                if (action.Parent is null)
                                    file.EventManager.AddFirst(action.Target.Clone());
                                else
                                    file.EventManager.AddAfter((int)action.Parent, action.Target.Clone());
                            }
                            newFuture.Add(new Commit<Event>(future, CommitActionType.Delete));
                        }
                        break;
                    case CommitActionType.Insert:
                        {
                            var future = new List<CommitAction<Event>>();
                            foreach (var action in commit.Actions)
                            {
                                future.Add(new CommitAction<Event>(action.Target.Clone(), action.Parent));
                                file.EventManager.Remove(action.Target.Id);
                            }
                            newFuture.Add(new Commit<Event>(future, CommitActionType.Insert));
                        }
                        break;
                }
            }
            file.HistoryManager.PushFuture(new CommitGroup<Event>(newFuture));
            UpToDate = false;
        }

        public void Redo()
        {
            if (!file.HistoryManager.EventCanGoForward) return;
            var futureCommitGroup = file.HistoryManager.EventGoForward();

            var newHistory = new List<Commit<Event>>();

            foreach (var commit in futureCommitGroup.Commits)
            {
                switch (commit.Type)
                {
                    case CommitActionType.Edit:
                        {
                            var history = new List<CommitAction<Event>>();
                            history.AddRange(commit.Actions.Select(a => new CommitAction<Event>(file.EventManager.Get(a.Target.Id).Clone(), a.Parent)));
                            file.EventManager.ReplaceInplace(commit.Actions.Select(a => a.Target.Clone()));
                            newHistory.Add(new Commit<Event>(history));
                        }
                        break;
                    case CommitActionType.Insert:
                        {
                            var history = new List<CommitAction<Event>>();
                            foreach (var action in commit.Actions)
                            {
                                history.Add(new CommitAction<Event>(action.Target.Clone(), action.Parent));
                                if (action.Parent == null)
                                    file.EventManager.AddFirst(action.Target.Clone());
                                else
                                    file.EventManager.AddAfter((int)action.Parent, action.Target);
                            }
                            newHistory.Add(new Commit<Event>(history, CommitActionType.Insert));
                        }
                        break;
                    case CommitActionType.Delete:
                        {
                            var history = new List<CommitAction<Event>>();
                            foreach (var action in commit.Actions)
                            {
                                history.Add(new CommitAction<Event>(action.Target.Clone(), action.Parent));
                                file.EventManager.Remove(action.Target.Id);
                            }
                            newHistory.Add(new Commit<Event>(history, CommitActionType.Delete));
                        }
                        break;
                }
            }
            file.HistoryManager.PushHistory(new CommitGroup<Event>(newHistory));
            UpToDate = false;
        }

        /// <summary>
        /// Select the next, or previous, or new event
        /// </summary>
        /// <param name="e">Current event</param>
        /// <returns>Event found</returns>
        private Event SelectAdjOrDefault(Event e)
        {
            var want = file.EventManager.GetAfter(e.Id);
            if (want == null) want = file.EventManager.GetBefore(e.Id);
            if (want == null)
            {
                var id = file.EventManager.AddFirst(new Event(file.EventManager.NextId));
                want = file.EventManager.Get(id);
            }
            return want;
        }

        #region Commands

        /// <summary>
        /// Duplicate the selected events.
        /// Each event will be duplicated; ABC → AABBCC
        /// </summary>
        public void DuplicateSelected()
        {
            if (SelectedEventCollection == null) return;
            var dupes = new List<Event>();
            foreach (var evnt in SelectedEventCollection)
            {
                dupes.Add(File.EventManager.Duplicate(evnt));
            }
            if (dupes.Count > 0)
                Add(dupes, dupes[0], false);
        }

        /// <summary>
        /// Insert a new event before the selected event
        /// </summary>
        public void InsertBeforeSelected()
        {
            if (SelectedEvent == null) return;
            var evnt = File.EventManager.InsertBefore(SelectedEvent);
            Add(new List<Event>() { evnt }, evnt, false);
        }

        /// <summary>
        /// Insert a new event after the selected event
        /// </summary>
        public void InsertAfterSelected()
        {
            if (SelectedEvent == null) return;
            var evnt = File.EventManager.InsertAfter(SelectedEvent);
            Add(new List<Event>() { evnt }, evnt, false);
        }

        /// <summary>
        /// Split an event on \N, CPS-adjusted
        /// </summary>
        public void SplitSelected()
        {
            if (SelectedEvent == null || SelectedEventCollection == null) return;
            SetSelectedEvents(SelectedEventCollection, SelectedEvent);

            var original = SelectedEvent;
            if (original == null) return;
            string[] delims = { "\\N", "\\n" };
            var segments = original.Text.Split(delims, StringSplitOptions.None);
            if (segments.Length == 0) return;

            var rollingTime = original.Start;
            var goalTime = original.End;

            Event prevEvent = original;
            Event newEvent;
            var newEvents = new List<Event>();
            foreach (var segment in segments)
            {
                newEvent = new Event(File.EventManager.NextId, original);
                var ratio = segment.Length / (double)original.Text.Replace("\\N", string.Empty).Replace("\\n", string.Empty).Length;
                newEvent.Text = segment;
                newEvent.Start = Time.FromTime(rollingTime);
                newEvent.End = rollingTime + Time.FromMillis(Convert.ToInt64((goalTime.TotalMilliseconds - original.Start.TotalMilliseconds) * ratio));
                if (newEvent.End > goalTime) newEvent.End = goalTime;

                File.EventManager.AddAfter(prevEvent?.Id ?? 0, newEvent);
                newEvents.Add(newEvent);
                prevEvent = newEvent;
                rollingTime = newEvent.End;
            }
            var addSnap = Add(newEvents, newEvents[0], true, false);
            var remSnap = Remove(new List<Event>() { original }, original, false);
            var snaps = new List<Commit<Event>>() { addSnap, remSnap };
            File.HistoryManager.Commit(new CommitGroup<Event>(snaps));
        }

        public void MergeSelectedAdj()
        {
            if (SelectedEvent == null || SelectedEventCollection == null) return;
            if (SelectedEventCollection.Count != 2) return;

            SetSelectedEvents(SelectedEventCollection, SelectedEvent);

            var one = SelectedEventCollection[0];
            var two = SelectedEventCollection[1];

            var afterOne = File.EventManager.GetAfter(one.Id);
            var beforeOne = File.EventManager.GetBefore(one.Id);

            if (afterOne != null && afterOne.Equals(two))
            {
                var result = new Event(File.EventManager.NextId, one)
                {
                    Start = one.Start,
                    End = two.End,
                    Text = $"{one.Text}{(UseSoftLinebreaks ? "\\n" : "\\N")}{two.Text}"
                };
                File.EventManager.AddAfter(two.Id, result);
                var remSnap = Remove(SelectedEventCollection, SelectedEvent, false);
                SelectedEvent = result;
                SelectedEventCollection.Clear();
                SelectedEventCollection.Add(result);
                var addSnap = Add(new List<Event>() { result }, result, true, false);
                var snaps = new List<Commit<Event>>() { addSnap, remSnap };
                File.HistoryManager.Commit(new CommitGroup<Event>(snaps));
            }
            else if (beforeOne != null && beforeOne.Equals(two))
            {
                var result = new Event(File.EventManager.NextId, one)
                {
                    Start = two.Start,
                    End = one.End,
                    Text = $"{two.Text}{(UseSoftLinebreaks ? "\\n" : "\\N")}{one.Text}"
                };
                File.EventManager.AddAfter(one.Id, result);
                var remSnap = Remove(SelectedEventCollection, SelectedEvent, false);
                SelectedEvent = result;
                SelectedEventCollection.Clear();
                SelectedEventCollection.Add(result);
                var addSnap = Add(new List<Event>() { result }, result, false, false);
                var snaps = new List<Commit<Event>>() { addSnap, remSnap };
                File.HistoryManager.Commit(new CommitGroup<Event>(snaps));
            }
            else return;
        }

        /// <summary>
        /// Select the next event, or create a new one if there is no subsequent event
        /// </summary>
        public void NextOrAdd()
        {
            if (SelectedEvent == null) return;

            var next = File.EventManager.GetAfter(SelectedEvent.Id);
            if (next != null)
            {
                SetSelectedEvents(new List<Event>() { next }, next);
            }
            else
            {
                next = new Event(File.EventManager.NextId)
                {
                    Style = SelectedEvent.Style,
                    Start = new Time(SelectedEvent.End),
                    End = new Time(SelectedEvent.End + Time.FromSeconds(5))
                };
                File.EventManager.AddLast(next);
                var list = new List<Event>() { next };
                Add(list, next, true);
            }
        }

        /// <summary>
        /// Propagate changes made to the selected event to all selected events
        /// </summary>
        /// <param name="selectionGroup">List of selected events</param>
        /// <param name="progenitor">Old version of the selected event</param>
        /// <param name="child">New version of the selected event</param>
        public void PropagateChanges(List<Event> selectionGroup, Event progenitor, Event child)
        {
            if (progenitor.Comment != child.Comment) selectionGroup.ForEach(e => e.Comment = child.Comment);
            if (progenitor.Layer != child.Layer) selectionGroup.ForEach(e => e.Layer = child.Layer);
            if (progenitor.Start != child.Start) selectionGroup.ForEach(e => e.Start = new Time(child.Start));
            if (progenitor.End != child.End) selectionGroup.ForEach(e => e.End = new Time(child.End));
            if (progenitor.Style != child.Style) selectionGroup.ForEach(e => e.Style = child.Style);
            if (progenitor.Actor != child.Actor) selectionGroup.ForEach(e => e.Actor = child.Actor);
            if (progenitor.Effect != child.Effect) selectionGroup.ForEach(e => e.Effect = child.Effect);
            if (progenitor.Text != child.Text) selectionGroup.ForEach(e => e.Text = child.Text);
            if (progenitor.Margins.Left != child.Margins.Left) selectionGroup.ForEach(e => e.Margins.Left = child.Margins.Left);
            if (progenitor.Margins.Right != child.Margins.Right) selectionGroup.ForEach(e => e.Margins.Right = child.Margins.Right);
            if (progenitor.Margins.Vertical != child.Margins.Vertical) selectionGroup.ForEach(e => e.Margins.Vertical = child.Margins.Vertical);
        }

        public (int, int) ToggleTag(string tag, int start, int end)
        {
            logger.Debug($"TOGGLE TAG start {start}, end {end}", "FileWrapper");
            if (SelectedEvent == null || SelectedEventCollection == null) return (start, end);
            Style? style = file.StyleManager.Get(SelectedEvent.Style);

            // Commit previous to history
            var sp = new CommitAction<Event>(SelectedEvent, file.EventManager.GetBefore(SelectedEvent.Id)?.Id);
            var s = new Commit<Event>(sp, CommitActionType.Edit);
            file.HistoryManager.Commit(new CommitGroup<Event>(s));
            SetSelectedEvents(SelectedEventCollection, SelectedEvent);

            var shift = SelectedEvent.ToggleTag(tag, style, start, end);
            return (start + shift, end + shift);
        }

        #endregion

        private static bool UseSoftLinebreaks
        {
            get
            {
                if (HoloContext.Instance.Workspace.UseSoftLinebreaks != null)
                    return HoloContext.Instance.Workspace.UseSoftLinebreaks ?? false;
                return HoloContext.Instance.ConfigurationManager?.UseSoftLinebreaks ?? false;
            }
        }

        public FileWrapper(File file, int id, Uri? filePath)
        {
            this.file = file;
            ID = id;
            UpToDate = true;
            FilePath = filePath;
            if (filePath != null) title = System.IO.Path.GetFileNameWithoutExtension(filePath.LocalPath);
            else title = $"New {id}";
            logger = HoloContext.Logger;

            // temp
            avManager = new AVManager();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

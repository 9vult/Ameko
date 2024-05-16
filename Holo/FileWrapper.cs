using AssCS;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

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

        private AVManager? avManager;

        private Logger logger;

        public File File => file;
        public int ID { get; }

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

        public AVManager? AVManager => avManager;

        private bool selecting = false; // TODO: There MUST be a better way to do this but I can't think of it right now
        public Snapshot<Event>? Select(List<Event> newSelectedEvents, Event newSelectedEvent, bool commit = true)
        {
            if (selecting) return null; // Prevent `SelectedEvent = newSelectedEvent;` from triggering another select event (pain-peko!!!)
            selecting = true;

            if (previouslySelectedEvent == null || previouslySelectedEvents == null)
            {
                // Don't check, just commit
                logger.Debug("SELECT→ Null Save", "FileWrapper");
                previouslySelectedEvent = newSelectedEvent.Clone();
                previouslySelectedEvents = newSelectedEvents.Select(e => e.Clone()).ToList();
                var snapshot = new Snapshot<Event>(
                    newSelectedEvents.Select(e => new SnapPosition<Event>(e.Clone(), file.EventManager.GetBefore(e.Id)?.Id)).ToList(),
                    AssCS.Action.EDIT);
                if (commit) file.HistoryManager.Commit(new Commit<Event>(snapshot));
                SelectedEvent = newSelectedEvent;
                SelectedEventCollection = new List<Event>(newSelectedEvents);
                selecting = false;
                return snapshot;
            }

            if (newSelectedEvents.Count == 0)
            {
                // Skip
                logger.Debug("SELECT→ Empty (skipping)", "FileWrapper");
                selecting = false;
                return null;
            }

            if (previouslySelectedEvent != null &&
                ((file.EventManager.Has(previouslySelectedEvent.Id) && previouslySelectedEvent.Equals(file.EventManager.Get(previouslySelectedEvent.Id)))
                || !file.EventManager.Has(previouslySelectedEvent.Id)))
            {
                // No change, continue
                logger.Debug("SELECT→ No change", "FileWrapper");
                previouslySelectedEvent = newSelectedEvent.Clone();
                previouslySelectedEvents = newSelectedEvents.Select(e => e.Clone()).ToList();
                SelectedEvent = newSelectedEvent;
                SelectedEventCollection = new List<Event>(newSelectedEvents);
                selecting = false;
                return null;
            }
            else
            {
                // Change happened, commit the Previously Selected lines
                logger.Debug("SELECT→ Committing changes", "FileWrapper");
                if (previouslySelectedEvents.Count > 1 && previouslySelectedEvent != null)
                {
                    var curPreviouslySelectedEvents = previouslySelectedEvents.Select(e => file.EventManager.Get(e.Id)).ToList();
                    var curPreviouslySelectedEvent = file.EventManager.Get(previouslySelectedEvent.Id);
                    PropagateChanges(curPreviouslySelectedEvents, previouslySelectedEvent, curPreviouslySelectedEvent);
                }
                var snapshot = new Snapshot<Event>(
                    previouslySelectedEvents.Select(e =>
                        new SnapPosition<Event>(
                            e.Clone(),
                            file.EventManager.GetBefore(e.Id)?.Id)
                        ).ToList(),
                    AssCS.Action.EDIT);
                if (commit) file.HistoryManager.Commit(new Commit<Event>(snapshot));
                previouslySelectedEvent = newSelectedEvent.Clone();
                previouslySelectedEvents = newSelectedEvents.Select(e => e.Clone()).ToList();
                SelectedEvent = newSelectedEvent;
                SelectedEventCollection = new List<Event>(newSelectedEvents);
                UpToDate = false;
                selecting = false;
                return snapshot;
            }
        }

        public Snapshot<Event> Remove(List<Event> selectedEvents, Event selectedEvent, bool commit = true)
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
            var snapshot = new Snapshot<Event>(
                selectedEvents.Select(e =>
                    new SnapPosition<Event>(e.Clone(), parents[e.Id])).ToList(),
                AssCS.Action.DELETE);
            if (commit) file.HistoryManager.Commit(new Commit<Event>(snapshot));
            file.EventManager.Remove(selectedEvent.Id);
            SelectedEventCollection = new List<Event> { want };
            SelectedEvent = want;
            UpToDate = false;
            return snapshot;
        }

        public Snapshot<Event> Add(List<Event> selectedEvents, Event selectedEvent, bool select, bool commit = true)
        {
            var snapshots = new List<Snapshot<Event>>();

            var snapshot = new Snapshot<Event>(
                selectedEvents.Select(e =>
                    new SnapPosition<Event>(e.Clone(), file.EventManager.GetBefore(e.Id)?.Id)).ToList(),
                AssCS.Action.INSERT);
            snapshots.Add(snapshot);

            if (select)
            {
                var selectSnapshot = Select(selectedEvents, selectedEvent, false);
                if (selectSnapshot != null) snapshots.Add(selectSnapshot);
            }

            if (commit) file.HistoryManager.Commit(new Commit<Event>(snapshots));
            if (select)
            {
                SelectedEvent = selectedEvent;
                SelectedEventCollection = selectedEvents;
            }
            UpToDate = false;
            return snapshot;
        }

        public void Undo()
        {
            if (!file.HistoryManager.EventCanGoBack) return;
            var historyCommit = file.HistoryManager.EventGoBack();
            if (historyCommit == null) return;

            var futureSnapshots = new List<Snapshot<Event>>();

            foreach (var hsnap in historyCommit.Snapshots)
            {
                switch (hsnap.action)
                {
                    case AssCS.Action.EDIT:
                        {
                            var fsnap = new List<SnapPosition<Event>>();

                            fsnap.AddRange(hsnap.snapshot.Select(sp => new SnapPosition<Event>(file.EventManager.Get(sp.Target.Id).Clone(), sp.Parent)));
                            file.EventManager.ReplaceInplace(hsnap.snapshot.Select(sp => sp.Target));

                            futureSnapshots.Add(new Snapshot<Event>(fsnap, AssCS.Action.EDIT));
                            break;
                        }

                    case AssCS.Action.DELETE:
                        {
                            var fsnap = new List<SnapPosition<Event>>();
                            foreach (var hpos in hsnap.snapshot)
                            {
                                fsnap.Add(new SnapPosition<Event>(hpos.Target.Clone(), hpos.Parent));
                                if (hpos.Parent == null) file.EventManager.AddFirst(hpos.Target);
                                else file.EventManager.AddAfter((int)hpos.Parent, hpos.Target);
                            }
                            futureSnapshots.Add(new Snapshot<Event>(fsnap, AssCS.Action.DELETE));
                            break;
                        }

                    case AssCS.Action.INSERT:
                        {
                            var fsnap = new List<SnapPosition<Event>>();
                            foreach (var hpos in hsnap.snapshot)
                            {
                                fsnap.Add(new SnapPosition<Event>(hpos.Target.Clone(), hpos.Parent));
                                file.EventManager.Remove(hpos.Target.Id);
                            }
                            futureSnapshots.Add(new Snapshot<Event>(fsnap, AssCS.Action.INSERT));
                            break;
                        }
                }
            }
            file.HistoryManager.PushFuture(new Commit<Event>(futureSnapshots));
            UpToDate = false;
        }

        public void Redo()
        {
            if (!file.HistoryManager.EventCanGoForward) return;
            var futurecommit = file.HistoryManager.EventGoForward();
            if (futurecommit == null) return;

            var historySnapshots = new List<Snapshot<Event>>();

            foreach (var fsnap in futurecommit.Snapshots)
            {
                switch (fsnap.action)
                {
                    case AssCS.Action.EDIT:
                        {
                            var hsnap = new List<SnapPosition<Event>>();

                            hsnap.AddRange(fsnap.snapshot.Select(sp => new SnapPosition<Event>(file.EventManager.Get(sp.Target.Id).Clone(), sp.Parent)));
                            file.EventManager.ReplaceInplace(fsnap.snapshot.Select(sp => sp.Target));

                            historySnapshots.Add(new Snapshot<Event>(hsnap, AssCS.Action.EDIT));
                            break;
                        }

                    case AssCS.Action.INSERT:
                        {
                            var hpos = new List<SnapPosition<Event>>();
                            foreach (var fpos in fsnap.snapshot)
                            {
                                hpos.Add(new SnapPosition<Event>(fpos.Target.Clone(), fpos.Parent));
                                if (fpos.Parent == null) file.EventManager.AddFirst(fpos.Target);
                                else file.EventManager.AddAfter((int)fpos.Parent, fpos.Target);
                            }
                            historySnapshots.Add(new Snapshot<Event>(hpos, AssCS.Action.INSERT));
                            break;
                        }

                    case AssCS.Action.DELETE:
                        {
                            var hpos = new List<SnapPosition<Event>>();
                            foreach (var fpos in fsnap.snapshot)
                            {
                                hpos.Add(new SnapPosition<Event>(fpos.Target.Clone(), fpos.Parent));
                                file.EventManager.Remove(fpos.Target.Id);
                            }
                            historySnapshots.Add(new Snapshot<Event>(hpos, AssCS.Action.DELETE));
                            break;
                        }
                }
            }
            file.HistoryManager.PushHistory(new Commit<Event>(historySnapshots));
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
            Select(SelectedEventCollection, SelectedEvent);

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
            var snaps = new List<Snapshot<Event>>() { addSnap, remSnap };
            File.HistoryManager.Commit(new Commit<Event>(snaps));
        }

        public void MergeSelectedAdj()
        {
            if (SelectedEvent == null || SelectedEventCollection == null) return;
            if (SelectedEventCollection.Count != 2) return;

            Select(SelectedEventCollection, SelectedEvent);

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
                var snaps = new List<Snapshot<Event>>() { addSnap, remSnap };
                File.HistoryManager.Commit(new Commit<Event>(snaps));
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
                var snaps = new List<Snapshot<Event>>() { addSnap, remSnap };
                File.HistoryManager.Commit(new Commit<Event>(snaps));
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
                Select(new List<Event>() { next }, next);
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
            var sp = new SnapPosition<Event>(SelectedEvent, file.EventManager.GetBefore(SelectedEvent.Id)?.Id);
            var s = new Snapshot<Event>(sp, AssCS.Action.EDIT);
            file.HistoryManager.Commit(new Commit<Event>(s));
            Select(SelectedEventCollection, SelectedEvent);

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

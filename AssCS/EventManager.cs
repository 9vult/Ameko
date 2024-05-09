using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace AssCS
{
    /// <summary>
    /// Manages the events in a file
    /// </summary>
    public class EventManager
    {
        private readonly LinkedList<int> chain;
        private readonly RangeObservableCollection<int> current;
        private readonly Dictionary<int, EventLink> events;
        private int _id = 0;

        /// <summary>
        /// Obtain the next available ID for use
        /// </summary>
        public int NextId => _id++;
        /// <summary>
        /// The first event
        /// </summary>
        public Event Head => events[chain.First.Value].Event;
        /// <summary>
        /// The last event
        /// </summary>
        public Event Tail => events[chain.Last.Value].Event;
        /// <summary>
        /// List of all events in order
        /// </summary>
        public List<Event> Ordered => chain.Select(id => events[id].Event).ToList();
        /// <summary>
        /// Observable collection of the current event IDs
        /// </summary>
        public RangeObservableCollection<int> CurrentEvents => current;
        /// <summary>
        /// Set containing currently-used actors
        /// </summary>
        public HashSet<string> Actors => events.Values.Select(el => el.Event.Actor).Where(a => !a.Equals(string.Empty)).ToHashSet();
        /// <summary>
        /// Set containing currently-used effects
        /// </summary>
        public HashSet<string> Effects => events.Values.Select(el => el.Event.Effect).Where(e => !e.Equals(string.Empty)).ToHashSet();

        /// <summary>
        /// Add an event after another event
        /// </summary>
        /// <param name="id">ID of the event to add after</param>
        /// <param name="e">Event to add</param>
        /// <returns>ID of the event added</returns>
        /// <exception cref="ArgumentException">If the ID is not found</exception>
        public int AddAfter(int id, Event e)
        {
            if (!events.ContainsKey(id)) throw new ArgumentException($"Cannot add event after id={id} because that id cannot be found.");

            var pre = events[id].Link;
            var link = chain.AddAfter(pre, e.Id);
            events[e.Id] = new EventLink(link, e);
            current.Add(e.Id);
            return e.Id;
        }

        /// <summary>
        /// Add multiple events after an event.
        /// Events will be added in the order dictated by the list.
        /// </summary>
        /// <param name="id">ID of the event to add after</param>
        /// <param name="list">Events to add</param>
        /// <returns>ID of the last event added</returns>
        /// <exception cref="ArgumentException">If the ID is not found</exception>
        public int AddAfter(int id, IEnumerable<Event> list)
        {
            if (!events.ContainsKey(id)) throw new ArgumentException($"Cannot add event after id={id} because that id cannot be found.");

            var previous = id;
            foreach (var e in list)
            {
                var pre = events[previous].Link;
                var link = chain.AddAfter(pre, e.Id);
                events[e.Id] = new EventLink(link, e);
                previous = e.Id;
            }

            current.AddRange(list.Select(el => el.Id));
            return list.Last().Id;
        }

        /// <summary>
        /// Add an event before another event
        /// </summary>
        /// <param name="id">ID of the event to add before</param>
        /// <param name="e">Event to add</param>
        /// <returns>ID of the event added</returns>
        /// <exception cref="ArgumentException">If the ID is not found</exception>
        public int AddBefore(int id, Event e)
        {
            if (!events.ContainsKey(id)) throw new ArgumentException($"Cannot add event before id={id} because that id cannot be found.");

            var post = events[id].Link;
            var link = chain.AddBefore(post, e.Id);
            events[e.Id] = new EventLink(link, e);
            current.Add(e.Id);
            return e.Id;
        }

        /// <summary>
        /// Add multiple events before another event
        /// </summary>
        /// <param name="id">ID of the event to add before</param>
        /// <param name="list">Events to add</param>
        /// <param name="ascending">True if the events should be added `C←B←A←Origin`, False if `A→B→C→Origin`</param>
        /// <returns>ID of the last event added</returns>
        /// <exception cref="ArgumentException">If the ID is not found</exception>
        public int AddBefore(int id, IEnumerable<Event> list, bool ascending)
        {
            if (!events.ContainsKey(id)) throw new ArgumentException($"Cannot add event before id={id} because that id cannot be found.");

            // Add in ascending order
            // C←B←A←Origin
            if (ascending)
            {
                var previous = id;
                foreach (var e in list)
                {
                    var post = events[previous].Link;
                    var link = chain.AddBefore(post, e.Id);
                    events[e.Id] = new EventLink(link, e);
                    previous = e.Id;
                }
            }
            // Add in descending order
            // A→B→C→Origin
            else
            {
                // Add first event
                int first = AddBefore(id, list.First());
                var previous = first;
                // Continue to add the remaining events after the first event
                foreach (var e in list.Skip(1))
                {
                    var pre = events[previous].Link;
                    var link = chain.AddAfter(pre, e.Id);
                    events[e.Id] = new EventLink(link, e);
                    previous = e.Id;
                }
            }

            current.AddRange(list.Select(el => el.Id));
            return list.Last().Id;
        }

        /// <summary>
        /// Add an event to the end
        /// </summary>
        /// <param name="e">Event to add</param>
        /// <returns>ID of the event</returns>
        public int AddLast(Event e)
        {
            var link = chain.AddLast(e.Id);
            events[e.Id] = new EventLink(link, e);
            current.Add(e.Id);
            return e.Id;
        }

        /// <summary>
        /// Add multiple events to the end
        /// </summary>
        /// <param name="list">Events to add</param>
        /// <returns>ID of the last event added</returns>
        public int AddLast(IEnumerable<Event> list)
        {
            foreach (var e in list)
            {
                var link = chain.AddLast(e.Id);
                events[e.Id] = new EventLink(link, e);
            }

            current.AddRange(list.Select(el => el.Id));
            return list.Last().Id;
        }

        /// <summary>
        /// Add an event to the beginning
        /// </summary>
        /// <param name="e">Event to add</param>
        /// <returns>ID of the event</returns>
        public int AddFirst(Event e)
        {
            var link = chain.AddFirst(e.Id);
            events[e.Id] = new EventLink(link, e);
            current.Add(e.Id);
            return e.Id;
        }

        /// <summary>
        /// Add multiple events to the beginning
        /// </summary>
        /// <param name="list">Events to add</param>
        /// <param name="ascending">True if the events should be added `C←B←A`, False if `A→B→C`</param>
        /// <returns>ID of the last event added</returns>
        public int AddFirst(IEnumerable<Event> list, bool ascending)
        {
            // Add in ascending order
            // C←B←A
            if (ascending)
            {
                foreach (var e in list)
                {
                    var link = chain.AddFirst(e.Id);
                    events[e.Id] = new EventLink(link, e);
                }
            }
            // Add in descending order
            // A→B→C
            else
            {
                int first = AddFirst(list.First());
                AddAfter(first, list.Skip(1));
            }

            current.AddRange(list.Select(el => el.Id));
            return list.Last().Id;
        }

        /// <summary>
        /// Inline-replace an event with another
        /// </summary>
        /// <param name="id">ID of the event to replace</param>
        /// <param name="e">Event to replace with</param>
        /// <returns>Original event</returns>
        /// <exception cref="ArgumentException">If the ID is not found</exception>
        public Event Replace(int id, Event e)
        {
            if (!events.ContainsKey(id)) throw new ArgumentException($"Cannot replace event id={id} because that id cannot be found.");

            var original = events[id];
            var originalEvent = original.Event;

            events.Remove(id);                              // Remove the original ID
            original.Link.Value = e.Id;                     // Set the LinkedList value to the new ID
            events[e.Id] = new EventLink(original.Link, e); // Add the new ID
            current[current.IndexOf(id)] = e.Id;            // Replace the ID in the current list

            return originalEvent;
        }

        /// <summary>
        /// Remove an event
        /// </summary>
        /// <param name="id">ID of the event to remove</param>
        /// <returns>True if the event was removed</returns>
        /// <exception cref="ArgumentException">If the ID is not found</exception>
        public bool Remove(int id)
        {
            if (events.ContainsKey(id))
            {
                var tup = events[id];
                events.Remove(id);
                chain.Remove(tup.Link);
                return current.Remove(id);
            }
            throw new ArgumentException($"Cannot remove event id={id} because that id cannot be found.");
        }

        /// <summary>
        /// Remove multiple events
        /// </summary>
        /// <param name="ids">IDs of the events to remove</param>
        /// <returns>True if all the events were removed</returns>
        public bool Remove(IEnumerable<int> ids)
        {
            if (ids.Count() == 0) return false;
            bool result = true;

            foreach (var id in ids)
            {
                if (events.ContainsKey(id))
                {
                    var tup = events[id];
                    events.Remove(id);
                    chain.Remove(tup.Link);
                } 
                else result = false;
            }
            current.RemoveRange(ids);
            return result;
        }

        /// <summary>
        /// Get an event by ID
        /// </summary>
        /// <param name="id">ID of the event</param>
        /// <returns>The event with the specified ID</returns>
        /// <exception cref="ArgumentException">If the ID is not found</exception>
        public Event Get(int id)
        {
            if (events.ContainsKey(id))
            {
                return events[id].Event;
            }
            throw new ArgumentException($"Cannot get event id={id} because that id cannot be found.");
        }

        /// <summary>
        /// Check if an event exists
        /// </summary>
        /// <param name="id">Event ID to check</param>
        /// <returns>True if it exists</returns>
        public bool Has(int id)
        {
            return events.ContainsKey(id);
        }

        /// <summary>
        /// Get the next event
        /// </summary>
        /// <param name="id">ID of the event before</param>
        /// <returns>The event after the event with the specified ID, or null</returns>
        public Event? GetAfter(int id)
        {
            if (events.ContainsKey(id))
            {
                if (events[id].Link.Next == null) return null;
                return events[events[id].Link.Next.Value].Event;
            }
            return null;
        }

        /// <summary>
        /// Get the previous event
        /// </summary>
        /// <param name="id">ID of the event after</param>
        /// <returns>The event before the event with the specified ID, or null</returns>
        public Event? GetBefore(int id)
        {
            if (events.ContainsKey(id))
            {
                if (events[id].Link.Previous == null) return null;
                return events[events[id].Link.Previous.Value].Event;
            }
            return null;
        }

        /// <summary>
        /// Clean house
        /// </summary>
        public void Clear()
        {
            events.Clear();
            chain.Clear();
            current.Clear();
            _id = 0;
        }

        public void LoadDefault()
        {
            Clear();
            _id = 0;
            AddFirst(new Event(NextId));
        }

        #region Commands

        /// <summary>
        /// Duplicate an event
        /// </summary>
        /// <param name="target">Event to duplicate</param>
        public Event Duplicate(Event target)
        {
            var newEvnt = new Event(NextId, target);
            AddAfter(target.Id, newEvnt);
            return newEvnt;
        }

        /// <summary>
        /// Insert a new event before the specified event.
        /// The new event's length will be limited if there is a prior event.
        /// </summary>
        /// <param name="target">Target event</param>
        public Event InsertBefore(Event target)
        {
            var newEvnt = new Event(NextId);
            var before = GetBefore(target.Id);
            if (before != null)
            {
                if ((target.Start - before.End).TotalSeconds < 5)
                    newEvnt.Start = before.End;
                else
                    newEvnt.Start = target.Start - Time.FromSeconds(5);
            }
            newEvnt.End = target.Start;
            newEvnt.Style = target.Style;

            AddBefore(target.Id, newEvnt);
            return newEvnt;
        }

        /// <summary>
        /// Insert a new event after the specified event.
        /// The new event's length will be limited if there is a subsequent event.
        /// </summary>
        /// <param name="target">Target event</param>
        public Event InsertAfter(Event target)
        {
            var newEvnt = new Event(NextId);
            var after = GetAfter(target.Id);
            if (after != null)
            {
                if ((after.Start - target.End).TotalSeconds < 5)
                    newEvnt.End = after.Start;
                else
                    newEvnt.End = target.End + Time.FromSeconds(5);
            }
            newEvnt.Start = target.End;
            newEvnt.Style = target.Style;

            AddAfter(target.Id, newEvnt);
            return newEvnt;
        }

        #endregion Commands

        public EventManager(EventManager source)
        {
            chain = new LinkedList<int>(source.chain);
            current = new RangeObservableCollection<int>(source.current);
            events = new Dictionary<int, EventLink>(source.events);
        }

        public EventManager(File source)
        {
            chain = new LinkedList<int>(source.EventManager.chain);
            current = new RangeObservableCollection<int>(source.EventManager.current);
            events = new Dictionary<int, EventLink>(source.EventManager.events);
        }

        public EventManager()
        {
            chain = new LinkedList<int>();
            current = new RangeObservableCollection<int>();
            events = new Dictionary<int, EventLink>();
        }

        private class EventLink : Tuple<LinkedListNode<int>, Event>
        {
            public EventLink(LinkedListNode<int> listItem, Event e) : base(listItem, e) { }

            public LinkedListNode<int> Link => Item1;
            public Event Event => Item2;
        }
    }
}

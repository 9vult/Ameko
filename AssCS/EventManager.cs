﻿using System;
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
        private readonly ObservableCollection<int> current;
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
        public ObservableCollection<int> CurrentEvents => current;

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
            return e.Id;
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

        public EventManager(EventManager source)
        {
            chain = new LinkedList<int>(source.chain);
            current = new ObservableCollection<int>(source.current);
            events = new Dictionary<int, EventLink>(source.events);
        }

        public EventManager(File source)
        {
            chain = new LinkedList<int>(source.EventManager.chain);
            current = new ObservableCollection<int>(source.EventManager.current);
            events = new Dictionary<int, EventLink>(source.EventManager.events);
        }

        public EventManager()
        {
            chain = new LinkedList<int>();
            current = new ObservableCollection<int>();
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

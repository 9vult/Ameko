// SPDX-License-Identifier: MPL-2.0

using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using AssCS.History;

namespace AssCS;

/// <summary>
/// Manages the events in a document
/// </summary>
public class EventManager : BindableBase
{
    private readonly LinkedList<int> _chain = [];
    private readonly RangeObservableCollection<int> _currentIds = [];
    private readonly Dictionary<int, Link> _events = [];
    private int _id = 0;

    /// <summary>
    /// Obtain the next available ID for use in the document
    /// </summary>
    /// <remarks>
    /// <para>
    /// As the event's ID is used for mapping out the file and for history
    /// operations, IDs cannot be reused.
    /// </para><para>
    /// In general, it can be assumed that any given event will retain its
    /// ID throughout its life, however not every ID may be present in the
    /// document at all times. In addition, all copies of an event should
    /// share an ID.
    /// </para><para>
    /// For example, an event that has been edited will have the same ID as
    /// the copy stored in the document's <see cref="History.HistoryManager"/>.
    /// </para>
    /// </remarks>
    public int NextId => _id++;

    /// <summary>
    /// A list of all events in order
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method collects all events in the order specified by the
    /// underlying LinkedList. Thus, the event order is determinate.
    /// </para><para>
    /// In addition, the <see cref="Event.Index"/> field is set to the event's
    /// row number (1-indexed).
    /// </para>
    /// </remarks>
    public List<Event> Events =>
        _chain
            .Select(
                (id, i) =>
                {
                    var e = _events[id].Event;
                    e.Index = i + 1;
                    return e;
                }
            )
            .ToList();

    /// <summary>
    /// The number of events currently in the document
    /// </summary>
    public int Count => _chain.Count;

    /// <summary>
    /// An observable collection of the IDs of the events currently
    /// in the document
    /// </summary>
    /// <remarks>
    /// This collection can be listened to to detect changes in the
    /// events in the document; For example, to re-render a subtitle grid.
    /// </remarks>
    public RangeObservableCollection<int> CurrentIds => _currentIds;

    /// <summary>
    /// A set of all the actor values currently present in the document
    /// </summary>
    public HashSet<string> Actors =>
        _events.Values.Select(l => l.Event.Actor).Where(a => !a.Equals(string.Empty)).ToHashSet();

    /// <summary>
    /// A set of all the effect values currently present in the document
    /// </summary>
    public HashSet<string> Effects =>
        _events.Values.Select(l => l.Event.Effect).Where(e => !e.Equals(string.Empty)).ToHashSet();

    /// <summary>
    /// The first event in the document
    /// </summary>
    public Event Head =>
        _chain.First is null
            ? throw new ArgumentNullException("Cannot access Head event because it is null")
            : _events[_chain.First.Value].Event;

    /// <summary>
    /// The last event in the document
    /// </summary>
    public Event Tail =>
        _chain.Last is null
            ? throw new ArgumentNullException("Cannot access Tail event because it is null")
            : _events[_chain.Last.Value].Event;

    #region Basic Actions

    /// <summary>
    /// Add an event after another event
    /// </summary>
    /// <param name="id">ID of the parent event</param>
    /// <param name="e">Event to add</param>
    /// <exception cref="ArgumentException">If the ID is not found</exception>
    /// <remarks>
    /// The new event will be inserted between the parent event and the parent's
    /// current child, if it has one.
    /// </remarks>
    public void AddAfter(int id, Event e)
    {
        if (!_events.TryGetValue(id, out Link value))
            throw new ArgumentException(
                $"Cannot add event after id={id} because that id cannot be found."
            );

        var pre = value.Node;
        var link = _chain.AddAfter(pre, e.Id);
        _events[e.Id] = new Link(link, e);
        _currentIds.Add(e.Id);
        Notify();
    }

    /// <summary>
    /// Add multiple events after an event
    /// </summary>
    /// <param name="id">ID of the parent event</param>
    /// <param name="list">List of events to add</param>
    /// <exception cref="ArgumentException">If the ID is not found</exception>
    /// <remarks>
    /// The events will be added in the order they are found in the list.
    /// </remarks>
    public void AddAfter(int id, IEnumerable<Event> list)
    {
        if (!_events.ContainsKey(id))
            throw new ArgumentException(
                $"Cannot add event after id={id} because that id cannot be found."
            );

        var previous = id;
        foreach (var e in list)
        {
            var pre = _events[previous].Node;
            var link = _chain.AddAfter(pre, e.Id);
            _events[e.Id] = new Link(link, e);
            previous = e.Id;
        }

        _currentIds.AddRange(list.Select(l => l.Id));
        Notify();
    }

    /// <summary>
    /// Add an event before another event
    /// </summary>
    /// <param name="id">ID of the child event</param>
    /// <param name="e">Event to add</param>
    /// <exception cref="ArgumentException">If the ID is not found</exception>
    /// <remarks>
    /// The new event will be inserted between the child event and the child's
    /// current parent, if it has one.
    /// </remarks>
    public void AddBefore(int id, Event e)
    {
        if (!_events.TryGetValue(id, out Link value))
            throw new ArgumentException(
                $"Cannot add event after id={id} because that id cannot be found."
            );

        var post = value.Node;
        var link = _chain.AddBefore(post, e.Id);
        _events[e.Id] = new Link(link, e);
        _currentIds.Add(e.Id);
        Notify();
    }

    /// <summary>
    /// Add multiple events before an event
    /// </summary>
    /// <param name="id">ID of the child event</param>
    /// <param name="list">Events to add</param>
    /// <param name="ascending">Order to add the events in</param>
    /// <exception cref="ArgumentException">If the ID is not found</exception>
    /// <remarks>
    /// The <paramref name="ascending"/> parameter controls the order the
    /// events will be added to the document.
    /// <list type="bullet">
    /// <item>
    /// If <paramref name="ascending"/> is <see langword="true"/>, items will
    /// be added in <c>C←B←A←Origin</c> order.
    /// </item>
    /// <item>
    /// If <paramref name="ascending"/> is <see langword="false"/>, items will
    /// be added in <c>A→B→C→Origin</c> order.
    /// </item>
    /// </list>
    /// </remarks>
    public void AddBefore(int id, IEnumerable<Event> list, bool ascending)
    {
        if (!_events.ContainsKey(id))
            throw new ArgumentException(
                $"Cannot add event before id={id} because that id cannot be found."
            );

        // Add in ascending order: C←B←A←Origin
        if (ascending)
        {
            var previous = id;
            foreach (var e in list)
            {
                var post = _events[previous].Node;
                var link = _chain.AddBefore(post, e.Id);
                _events[e.Id] = new Link(link, e);
                previous = e.Id;
            }
        }
        // Add in descending order: A→B→C→Origin
        else
        {
            // Add first event
            AddBefore(id, list.First());
            var previous = list.First().Id;
            // Continue to add the remaining events after the first event
            foreach (var e in list.Skip(1))
            {
                var pre = _events[previous].Node;
                var link = _chain.AddAfter(pre, e.Id);
                _events[e.Id] = new Link(link, e);
                previous = e.Id;
            }
        }

        _currentIds.AddRange(list.Select(el => el.Id));
        Notify();
    }

    /// <summary>
    /// Add an event to the end of the document
    /// </summary>
    /// <param name="e">Event to add</param>
    /// <remarks>
    /// This event will become the new <see cref="Tail"/>.
    /// </remarks>
    public void AddLast(Event e)
    {
        var link = _chain.AddLast(e.Id);
        _events[e.Id] = new Link(link, e);
        _currentIds.Add(e.Id);
        Notify();
    }

    /// <summary>
    /// Add multiple events to the end of the document
    /// </summary>
    /// <param name="list">Events to add</param>
    /// <remarks>
    /// The last event in the <paramref name="list"/> will become
    /// the new <see cref="Tail"/>.
    /// </remarks>
    public void AddLast(IEnumerable<Event> list)
    {
        foreach (var e in list)
        {
            var link = _chain.AddLast(e.Id);
            _events[e.Id] = new Link(link, e);
        }

        _currentIds.AddRange(list.Select(el => el.Id));
        Notify();
    }

    /// <summary>
    /// Add an event to the beginning of the document
    /// </summary>
    /// <param name="e">Event to add</param>
    /// <remarks>
    /// This event will become the new <see cref="Head"/>.
    /// </remarks>
    public void AddFirst(Event e)
    {
        var link = _chain.AddFirst(e.Id);
        _events[e.Id] = new Link(link, e);
        _currentIds.Add(e.Id);
        Notify();
    }

    /// <summary>
    /// Add multiple events to the beginning of the document
    /// </summary>
    /// <param name="list">Events to add</param>
    /// <param name="ascending">Order to add the events in</param>
    /// <remarks>
    /// The <paramref name="ascending"/> parameter controls the order the
    /// events will be added to the document.
    /// <list type="bullet">
    /// <item>
    /// If <paramref name="ascending"/> is <see langword="true"/>, items will
    /// be added in <c>C←B←A</c> order, and the <i>last</i> event in the
    /// <paramref name="list"/> will be the new <see cref="Head"/>
    /// of the document.
    /// </item>
    /// <item>
    /// If <paramref name="ascending"/> is <see langword="false"/>, items will
    /// be added in <c>A→B→C</c> order, and the <i>first</i> event in the
    /// <paramref name="list"/> will be the new <see cref="Head"/>
    /// of the document.
    /// </item>
    /// </list>
    /// </remarks>
    public void AddFirst(IEnumerable<Event> list, bool ascending)
    {
        // Add in ascending order: C←B←A
        if (ascending)
        {
            foreach (var e in list)
            {
                var link = _chain.AddFirst(e.Id);
                _events[e.Id] = new Link(link, e);
            }
        }
        // Add in descending order: A→B→C
        else
        {
            AddFirst(list.First());
            AddAfter(list.First().Id, list.Skip(1));
        }

        _currentIds.AddRange(list.Select(el => el.Id));
        Notify();
    }

    /// <summary>
    /// Replace an event with another
    /// </summary>
    /// <param name="id">ID of the event to replace</param>
    /// <param name="e">New event</param>
    /// <returns>The replaced event</returns>
    /// <exception cref="ArgumentException">If the ID is not found</exception>
    public Event Replace(int id, Event e)
    {
        if (!_events.TryGetValue(id, out Link link))
            throw new ArgumentException(
                $"Cannot replace event id={id} because that id cannot be found."
            );

        var originalEvent = link.Event;

        _events.Remove(id); // Remove the original ID
        link.Node.Value = e.Id; // Set the Node's value to the new ID
        _events[e.Id] = new Link(link.Node, e); // Add the new ID
        _currentIds[_currentIds.IndexOf(id)] = id; // Replace the ID in the list
        Notify();
        return originalEvent;
    }

    /// <summary>
    /// Replace an event with a new version of itself
    /// </summary>
    /// <param name="e">New event</param>
    /// <exception cref="ArgumentException">If the ID is not found</exception>
    public void ReplaceInplace(Event e)
    {
        if (!_events.TryGetValue(e.Id, out Link original))
            throw new ArgumentException(
                $"Cannot replace event id={e.Id} because that id cannot be found."
            );

        original.Node.Value = e.Id; // Set the LinkedList value to the new ID
        _events[e.Id] = new Link(original.Node, e); // Add the new ID

        Notify();
    }

    /// <summary>
    /// Replace events with new versions of themselves
    /// </summary>
    /// <param name="list">New events</param>
    /// <exception cref="ArgumentException">If an ID is not found</exception>
    public void ReplaceInplace(IEnumerable<Event> list)
    {
        foreach (var e in list)
        {
            if (!_events.TryGetValue(e.Id, out Link original))
                throw new ArgumentException(
                    $"Cannot replace event id={e.Id} because that id cannot be found."
                );

            original.Node.Value = e.Id; // Set the LinkedList value to the new ID
            _events[e.Id] = new Link(original.Node, e); // Add the new ID
        }
        Notify();
    }

    /// <summary>
    /// Remove an event from the document
    /// </summary>
    /// <param name="id">ID of the event to remove</param>
    /// <returns><see langword="true"/> if the event was removed</returns>
    /// <exception cref="ArgumentException">If the ID is not found</exception>
    public bool Remove(int id)
    {
        if (_events.TryGetValue(id, out Link link))
        {
            _events.Remove(id);
            _chain.Remove(link.Node);
            var result = _currentIds.Remove(id);
            Notify();
            return result;
        }
        throw new ArgumentException(
            $"Cannot remove event id={id} because that id cannot be found."
        );
    }

    /// <summary>
    /// Remove multiple events from the document
    /// </summary>
    /// <param name="list">IDs to remove</param>
    /// <returns><see langword="true"/> if all events were removed</returns>
    public bool Remove(IEnumerable<int> list)
    {
        if (!list.Any())
            return false;
        bool result = true;

        foreach (var id in list)
        {
            if (_events.TryGetValue(id, out Link link))
            {
                _events.Remove(id);
                _chain.Remove(link.Node);
            }
            else
                result = false;
        }
        _currentIds.RemoveRange(list);
        Notify();
        return result;
    }

    #endregion Basic Actions
    #region Getters

    /// <summary>
    /// Check if an event exists in the document
    /// </summary>
    /// <param name="id">ID to check</param>
    /// <returns><see langword="true"/> if the ID exists in the document</returns>
    public bool Has(int id)
    {
        return _events.ContainsKey(id);
    }

    /// <summary>
    /// Get an event by its ID
    /// </summary>
    /// <param name="id">ID of the event</param>
    /// <returns>The event with the requested ID</returns>
    /// <exception cref="ArgumentException">If the ID is not found</exception>
    public Event Get(int id)
    {
        if (_events.TryGetValue(id, out Link link))
        {
            return link.Event;
        }
        throw new ArgumentException($"Cannot get event id={id} because that id cannot be found.");
    }

    /// <summary>
    /// Get an event by its iD
    /// </summary>
    /// <param name="id">ID of the event</param>
    /// <param name="event">The event with the requested ID, if it exists</param>
    /// <returns><see langword="true"/> if the event exists in the document; otherwise, <see langword="false"/></returns>
    public bool TryGet(int id, [MaybeNullWhen(false)] out Event @event)
    {
        var found = _events.TryGetValue(id, out var value);
        @event = found ? value.Event : null;
        return found;
    }

    /// <summary>
    /// Get the event after an event
    /// </summary>
    /// <param name="id">ID of the parent event</param>
    /// <returns>The event after the parent, or <see langword="null"/>
    /// if the parent is the last in the document</returns>
    public Event? GetAfter(int id)
    {
        if (_events.TryGetValue(id, out Link link))
        {
            if (link.Node.Next == null)
                return null;
            return _events[link.Node.Next.Value].Event;
        }
        return null;
    }

    /// <summary>
    /// Get the event after an event
    /// </summary>
    /// <param name="id">ID of the parent event</param>
    /// <param name="event">The event after the parent, if it exists</param>
    /// <returns><see langword="true"/> if there is an event after the parent; otherwise, <see langword="false"/></returns>
    public bool TryGetAfter(int id, [MaybeNullWhen(false)] out Event @event)
    {
        if (
            _events.TryGetValue(id, out var preceeding)
            && preceeding.Node.Next is not null
            && _events.TryGetValue(preceeding.Node.Next.Value, out var value)
        )
        {
            @event = value.Event;
            return true;
        }

        @event = null;
        return false;
    }

    /// <summary>
    /// Get the event before an event
    /// </summary>
    /// <param name="id">ID of the child event</param>
    /// <returns>The event before the child, or <see langword="null"/>
    /// if the child is the first in the document</returns>
    public Event? GetBefore(int id)
    {
        if (_events.TryGetValue(id, out Link link))
        {
            if (link.Node.Previous == null)
                return null;
            return _events[link.Node.Previous.Value].Event;
        }
        return null;
    }

    /// <summary>
    /// Get the event before an event
    /// </summary>
    /// <param name="id">ID of the child event</param>
    /// <param name="event">The event before the child, if it exists</param>
    /// <returns><see langword="true"/> if there is an event before the child; otherwise, <see langword="false"/></returns>
    public bool TryGetBefore(int id, [MaybeNullWhen(false)] out Event @event)
    {
        if (
            _events.TryGetValue(id, out var following)
            && following.Node.Previous is not null
            && _events.TryGetValue(following.Node.Previous.Value, out var value)
        )
        {
            @event = value.Event;
            return true;
        }

        @event = null;
        return false;
    }

    #endregion Getters
    #region Advanced Actions

    /// <summary>
    /// Change the style name of all events with
    /// the given<paramref name="oldName"/>
    /// </summary>
    /// <param name="oldName">Name to replace</param>
    /// <param name="newName">New style name</param>
    /// <remarks>
    /// This method is recommended for use when the name of a style changes.
    /// </remarks>
    public void ChangeStyle(string oldName, string newName)
    {
        foreach (var e in _events.Values.Where(l => l.Event.Style == oldName))
        {
            e.Event.Style = newName;
        }
    }

    /// <summary>
    /// Duplicate an event, placing the clone after the existing one
    /// </summary>
    /// <param name="target">Event to duplicate</param>
    /// <returns>The duplicate event</returns>
    public Event Duplicate(Event target)
    {
        var newEvent = target.Clone(NextId);
        AddAfter(target.Id, newEvent);
        return newEvent;
    }

    /// <summary>
    /// Insert a new event before the target event
    /// </summary>
    /// <param name="target">Child event</param>
    /// <returns>The newly created event</returns>
    /// <remarks>
    /// The inserted event's length will be limited if there is an event
    /// before the <paramref name="target"/> event that would overlap with
    /// the creation of a full-length event.
    /// </remarks>
    public Event InsertBefore(Event target)
    {
        var newEvent = new Event(NextId)
        {
            Style = target.Style,
            End = Time.FromTime(target.Start),
        };

        var before = GetBefore(target.Id);
        if (before is not null)
        {
            if ((target.Start - before.End).TotalSeconds < 5)
                newEvent.Start = before.End;
            else
                newEvent.Start = target.Start - Time.FromSeconds(5);
        }
        else
        {
            newEvent.Start = target.Start - Time.FromSeconds(5);
        }
        AddBefore(target.Id, newEvent);
        return newEvent;
    }

    /// <summary>
    /// Insert a new event after the target event
    /// </summary>
    /// <param name="target">Parent event</param>
    /// <returns>The newly created event</returns>
    /// <remarks>
    /// The inserted event's length will be limited if there is an event
    /// after the <paramref name="target"/> event that would overlap with
    /// the creation of a full-length event.
    /// </remarks>
    public Event InsertAfter(Event target)
    {
        var newEvent = new Event(NextId)
        {
            Style = target.Style,
            Start = Time.FromTime(target.End),
        };

        var after = GetAfter(target.Id);
        if (after is not null)
        {
            if ((after.Start - target.End).TotalSeconds < 5)
                newEvent.End = after.Start;
            else
                newEvent.End = target.End + Time.FromSeconds(5);
        }
        else
        {
            newEvent.End = target.End + Time.FromSeconds(5);
        }
        AddAfter(target.Id, newEvent);
        return newEvent;
    }

    #endregion Advanced Actions

    private void Notify()
    {
        RaisePropertyChanged(nameof(Events));
    }

    private struct Link(LinkedListNode<int> node, Event e)
    {
        public LinkedListNode<int> Node = node;
        public Event Event = e;
    }
}

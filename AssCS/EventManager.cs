// SPDX-License-Identifier: MPL-2.0

using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using AssCS.Utilities;

namespace AssCS;

/// <summary>
/// Provides mechanisms for creating, retrieving, ordering,
/// and modifying subtitle <see cref="Event"/>s within a <see cref="Document"/>
/// </summary>
/// <remarks>
/// Events are uniquely identified and maintained in a logical order
/// for editing, rendering, and history tracking
/// </remarks>
public class EventManager : BindableBase
{
    private readonly LinkedList<int> _chain;
    private readonly RangeObservableCollection<int> _currentIds;
    private readonly ConcurrentDictionary<int, Link> _events;
    private int _id;

    /// <summary>
    /// Obtain the next available event ID for use in the document
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
    /// Gets a list of all current events in the document in visual order
    /// </summary>
    /// <remarks>
    /// The order is determined by the internal LinkedList structure.
    /// This accessor also assigns each event its current <see cref="Event.Index"/> (1-based)
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
    /// This collection can be observed to detect changes in the
    /// events in the document; For example, to re-render a subtitle grid.
    /// </remarks>
    public Utilities.ReadOnlyObservableCollection<int> CurrentIds { get; }

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
    /// <exception cref="ArgumentNullException">If the head is <see langword="null"/></exception>
    public Event Head =>
        _chain.First is null
            ? throw new ArgumentNullException(
                nameof(Head),
                "Cannot access Head event because it is null"
            )
            : _events[_chain.First.Value].Event;

    /// <summary>
    /// The last event in the document
    /// </summary>
    /// <exception cref="ArgumentNullException">If the tail is <see langword="null"/></exception>
    public Event Tail =>
        _chain.Last is null
            ? throw new ArgumentNullException(
                nameof(Tail),
                "Cannot access Tail event because it is null"
            )
            : _events[_chain.Last.Value].Event;

    #region Basic Actions

    /// <summary>
    /// Add an event after another event
    /// </summary>
    /// <param name="id">ID of the parent event</param>
    /// <param name="e">Event to add</param>
    /// <exception cref="ArgumentException">If the <paramref name="id"/> is not found</exception>
    /// <remarks>
    /// The new event will be inserted between the parent event and the parent's
    /// current child, if it has one.
    /// </remarks>
    /// <example>
    /// Insert a new event after an existing one:
    /// <code>
    /// var newEvent = new Event(manager.NextId) { Text = "New line" };
    /// manager.AddAfter(existingEvent.Id, newEvent);
    /// </code>
    /// </example>
    public void AddAfter(int id, Event e)
    {
        if (!_events.TryGetValue(id, out var value))
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
    /// <exception cref="ArgumentException">If the <paramref name="id"/> is not found</exception>
    /// <remarks>
    /// The events will be added in the order they are found in the list.
    /// </remarks>
    /// <example>
    /// Insert multiple new events after an existing one:
    /// <code>
    /// List&lt;Event&gt; newEvents = [new Event(manager.NextId), new Event(Manager.NextId)];
    /// manager.AddAfter(existingEvent.Id, newEvents);
    /// </code>
    /// </example>
    public void AddAfter(int id, IList<Event> list)
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
    /// <exception cref="ArgumentException">If the <paramref name="id"/> is not found</exception>
    /// <remarks>
    /// The new event will be inserted between the child event and the child's
    /// current parent, if it has one.
    /// </remarks>
    /// <example>
    /// Insert a new event before an existing one:
    /// <code>
    /// var newEvent = new Event(manager.NextId) { Text = "New line" };
    /// manager.AddBefore(existingEvent.Id, newEvent);
    /// </code>
    /// </example>
    public void AddBefore(int id, Event e)
    {
        if (!_events.TryGetValue(id, out var value))
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
    /// <exception cref="ArgumentException">If the <paramref name="id"/> is not found</exception>
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
    /// <example>
    /// Insert multiple new events before an existing one, in descending order:
    /// <code>
    /// List&lt;Event&gt; newEvents = [new Event(manager.NextId), new Event(Manager.NextId)];
    /// manager.AddBefore(existingEvent.Id, newEvents, ascending: false);
    /// </code>
    /// </example>
    public void AddBefore(int id, IList<Event> list, bool ascending)
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
    public void AddLast(IList<Event> list)
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
    public void AddFirst(IList<Event> list, bool ascending)
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
            AddAfter(list.First().Id, list.Skip(1).ToList());
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
    /// <remarks>
    /// The replacement event must have a unique ID that differs from the original.
    /// To update an event in-place with the same ID, use <see cref="ReplaceInPlace(Event)"/>.
    /// </remarks>
    /// <exception cref="ArgumentException">If the <paramref name="id"/> is not found</exception>
    /// <seealso cref="ReplaceInPlace(Event)"/>
    public Event Replace(int id, Event e)
    {
        if (!_events.TryRemove(id, out var link)) // Remove the original ID
            throw new ArgumentException(
                $"Cannot replace event id={id} because that id cannot be found."
            );

        var originalEvent = link.Event;

        link.Node.Value = e.Id; // Set the Node's value to the new ID
        _events[e.Id] = new Link(link.Node, e); // Add the new ID
        _currentIds[_currentIds.IndexOf(id)] = id; // Replace the ID in the list
        Notify();
        return originalEvent;
    }

    /// <summary>
    /// Replace an event with a new version of itself
    /// </summary>
    /// <param name="event">New event</param>
    /// <remarks>
    /// The replacement event must have the same ID as the original.
    /// To replace the event with one with a different ID, use <see cref="Replace(int, Event)"/>
    /// </remarks>
    /// <exception cref="ArgumentException">If the <paramref name="event"/> is not found</exception>
    /// <seealso cref="Replace(int, Event)"/>
    public void ReplaceInPlace(Event @event)
    {
        if (!_events.TryGetValue(@event.Id, out var original))
            throw new ArgumentException(
                $"Cannot replace event id={@event.Id} because that id cannot be found."
            );

        original.Node.Value = @event.Id; // Set the LinkedList value to the new ID
        _events[@event.Id] = new Link(original.Node, @event); // Add the new ID

        Notify();
    }

    /// <summary>
    /// Replace events with new versions of themselves
    /// </summary>
    /// <param name="list">New events</param>
    /// <remarks>
    /// The replacement events must have the same ID as the originals.
    /// To replace events with ones with a different ID, use <see cref="Replace(int, Event)"/>
    /// </remarks>
    /// <exception cref="ArgumentException">If an event in the <paramref name="list"/> is not found</exception>
    /// <seealso cref="Replace(int, Event)"/>
    public void ReplaceInPlace(IEnumerable<Event> list)
    {
        foreach (var e in list)
        {
            if (!_events.TryGetValue(e.Id, out var original))
                throw new ArgumentException(
                    $"Cannot replace event id={e.Id} because that id cannot be found."
                );

            original.Event.SetFields(EventField.All, e);
        }
        Notify();
    }

    /// <summary>
    /// Remove an event from the document
    /// </summary>
    /// <param name="id">ID of the event to remove</param>
    /// <returns><see langword="true"/> if the event was removed</returns>
    /// <exception cref="ArgumentException">If the <paramref name="id"/> is not found</exception>
    public bool Remove(int id)
    {
        if (_events.Remove(id, out var link))
        {
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
    public bool Remove(IList<int> list)
    {
        if (!list.Any())
            return false;
        var result = true;

        foreach (var id in list)
        {
            if (_events.Remove(id, out var link))
            {
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
    /// <exception cref="ArgumentException">If the <paramref name="id"/> is not found</exception>
    public Event Get(int id)
    {
        if (_events.TryGetValue(id, out var link))
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
    /// <returns><see langword="true"/> if the event exists in the document;
    /// otherwise, <see langword="false"/></returns>
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
        if (!_events.TryGetValue(id, out var link))
            return null;
        return link.Node.Next is not null ? _events[link.Node.Next.Value].Event : null;
    }

    /// <summary>
    /// Get the event after an event
    /// </summary>
    /// <param name="id">ID of the parent event</param>
    /// <param name="event">The event after the parent, if it exists</param>
    /// <returns><see langword="true"/> if there is an event after the parent;
    /// otherwise, <see langword="false"/></returns>
    public bool TryGetAfter(int id, [MaybeNullWhen(false)] out Event @event)
    {
        if (
            _events.TryGetValue(id, out var preceding)
            && preceding.Node.Next is not null
            && _events.TryGetValue(preceding.Node.Next.Value, out var value)
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
        if (!_events.TryGetValue(id, out var link))
            return null;
        return link.Node.Previous is not null ? _events[link.Node.Previous.Value].Event : null;
    }

    /// <summary>
    /// Get the event before an event
    /// </summary>
    /// <param name="id">ID of the child event</param>
    /// <param name="event">The event before the child, if it exists</param>
    /// <returns><see langword="true"/> if there is an event before the child;
    /// otherwise, <see langword="false"/></returns>
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

    /// <summary>
    /// Get the event after an event, creating it if needed
    /// </summary>
    /// <param name="id">ID of the parent event</param>
    /// <returns>The event after the parent</returns>
    /// <exception cref="ArgumentOutOfRangeException">If the <paramref name="id"/> is not found</exception>
    public Event GetOrCreateAfter(int id)
    {
        if (TryGetAfter(id, out var after))
            return after;

        // Create a new event
        if (!TryGet(id, out var @event))
            throw new ArgumentOutOfRangeException(nameof(id));

        after = new Event(NextId)
        {
            Style = @event.Style,
            Start = Time.FromTime(@event.End),
            End = @event.End + Time.FromSeconds(5),
        };

        AddLast(after);
        return after;
    }

    /// <summary>
    /// Get the event before an event, creating it if needed
    /// </summary>
    /// <param name="id">ID of the child event</param>
    /// <returns>The event before the child</returns>
    /// <exception cref="ArgumentOutOfRangeException">If the <paramref name="id"/> is not found</exception>
    public Event GetOrCreateBefore(int id)
    {
        if (TryGetBefore(id, out var before))
            return before;

        // Create a new event
        if (!TryGet(id, out var @event))
            throw new ArgumentOutOfRangeException(nameof(id));

        before = new Event(NextId)
        {
            Style = @event.Style,
            Start = Time.FromTime(@event.Start),
            End = Time.FromMillis(
                Math.Max(0, (@event.Start - Time.FromSeconds(5)).TotalMilliseconds)
            ),
        };

        AddFirst(before);
        return before;
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

    /// <summary>
    /// Split an event into multiple parts
    /// </summary>
    /// <remarks>
    /// Segments are computed based on `\N` or `\n` line breaks.
    /// Timing is adjusted proportionally based on characters per second.
    /// </remarks>
    /// <param name="id">ID of the event to split</param>
    /// <param name="keepTimes">All resulting events have the same start and end times?</param>
    /// <returns>List of events created by the split</returns>
    public IEnumerable<Event> Split(int id, bool keepTimes = false)
    {
        if (!TryGet(id, out var @event) || string.IsNullOrEmpty(@event.Text))
            return [];

        string[] delimiters = ["\\N", "\\n"];
        var segments = @event.Text.Split(delimiters, StringSplitOptions.None);
        if (segments.Length == 0)
            return [];

        List<Event> result = [];

        var rollingTime = @event.Start;
        var goalTime = @event.End;

        var previous = @event;
        foreach (var segment in segments)
        {
            var newEvent = Event.FromEvent(NextId, @event);
            var ratio =
                segment.Length / (double)@event.Text.ReplaceMany(delimiters, string.Empty).Length;

            newEvent.Text = segment;
            if (!keepTimes) // Calculate start/end times using CPS
            {
                newEvent.Start = Time.FromTime(rollingTime);
                newEvent.End =
                    rollingTime
                    + Time.FromMillis(
                        Math.Min(
                            Convert.ToInt64((goalTime - @event.Start).TotalMilliseconds * ratio),
                            goalTime.TotalMilliseconds
                        )
                    );
                rollingTime = newEvent.End;
            }

            AddAfter(previous.Id, newEvent);
            result.Add(newEvent);
            previous = newEvent;
        }

        result.Last().End = goalTime;

        Remove(@event.Id);
        return result;
    }

    /// <summary>
    /// Split an event into two parts about an index
    /// </summary>
    /// <param name="id">ID of the event to split</param>
    /// <param name="index">Character index to split at</param>
    /// <param name="keepTimes">All resulting events have the same start and end times?</param>
    /// <param name="splitTime">Optionally specify a time to use</param>
    /// <returns>List of events created by the split</returns>
    public IEnumerable<Event> Split(
        int id,
        int index,
        bool keepTimes = false,
        Time? splitTime = null
    )
    {
        if (!TryGet(id, out var @event) || string.IsNullOrEmpty(@event.Text))
            return [];

        if (keepTimes && splitTime is not null)
            throw new ArgumentException("Can't keep times with a specified split time!");

        List<string> segments = [@event.Text[..index], @event.Text[index..]];

        List<Event> result = [];

        // If a split time has been specified, just use that
        if (splitTime is not null)
        {
            if (splitTime > @event.End)
                splitTime = @event.End;
            if (splitTime < @event.Start)
                splitTime = @event.Start;

            var eventA = Event.FromEvent(NextId, @event);
            var eventB = Event.FromEvent(NextId, @event);

            eventA.Text = segments[0];
            eventB.Text = segments[1];
            eventA.End = splitTime;
            eventB.Start = splitTime;
            result.Add(eventA);
            result.Add(eventB);

            AddAfter(id, eventA);
            AddAfter(eventA.Id, eventB);
            Remove(id);
            return result;
        }

        var rollingTime = @event.Start;
        var goalTime = @event.End;

        var previous = @event;
        foreach (var segment in segments)
        {
            var newEvent = Event.FromEvent(NextId, @event);
            var ratio = segment.Length / (double)@event.Text.Length;

            newEvent.Text = segment;
            if (!keepTimes) // Calculate start/end times using CPS
            {
                newEvent.Start = Time.FromTime(rollingTime);
                newEvent.End =
                    rollingTime
                    + Time.FromMillis(
                        Math.Min(
                            Convert.ToInt64((goalTime - @event.Start).TotalMilliseconds * ratio),
                            goalTime.TotalMilliseconds
                        )
                    );
                rollingTime = newEvent.End;
            }

            AddAfter(previous.Id, newEvent);
            result.Add(newEvent);
            previous = newEvent;
        }

        result.Last().End = goalTime;

        Remove(@event.Id);
        return result;
    }

    /// <summary>
    /// Merge two adjacent events together
    /// </summary>
    /// <remarks>The events must be directly adjacent</remarks>
    /// <param name="a">IDs of the first event</param>
    /// <param name="b">IDs of the second event</param>
    /// <param name="useSoftLinebreaks">Whether to use soft linebreaks or not</param>
    /// <returns>The new event, or <see langword="null"/> on failure</returns>
    /// <remarks>The events must be adjacent. Non-adjacency will result in failure.</remarks>
    public Event? Merge(int a, int b, bool useSoftLinebreaks = false)
    {
        var newline = !useSoftLinebreaks ? @"\N" : @"\n";

        if (!TryGet(a, out var first))
            return null;
        if (!TryGet(b, out var second))
            return null;

        var afterFirst = GetAfter(a);
        var beforeFirst = GetBefore(a);

        Event result;
        if (afterFirst is not null && afterFirst.Equals(second))
        {
            result = Event.FromEvent(NextId, first);
            result.Start = first.Start;
            result.End = second.End;
            result.Text = $"{first.Text}{newline}{second.Text}";

            AddAfter(b, result);
            Remove(a);
            Remove(b);
            return result;
        }

        if (beforeFirst is null || !beforeFirst.Equals(second))
            return null;

        result = Event.FromEvent(NextId, first);
        result.Start = second.Start;
        result.End = first.End;
        result.Text = $"{second.Text}{newline}{first.Text}";
        AddAfter(a, result);
        Remove(a);
        Remove(b);
        return result;
    }

    #endregion Advanced Actions

    /// <summary>
    /// Clear everything and load in a default event
    /// </summary>
    public void LoadDefault()
    {
        _chain.Clear();
        _events.Clear();
        _currentIds.Clear();
        _id = 0;

        AddFirst(new Event(NextId));
        Notify();
    }

    private void Notify()
    {
        RaisePropertyChanged(nameof(Events));
    }

    public EventManager()
    {
        _chain = [];
        _currentIds = [];
        _events = [];

        CurrentIds = new Utilities.ReadOnlyObservableCollection<int>(_currentIds);
    }

    private struct Link(LinkedListNode<int> node, Event e)
    {
        public readonly LinkedListNode<int> Node = node;
        public readonly Event Event = e;
    }
}

// SPDX-License-Identifier: MPL-2.0

using System.Collections.ObjectModel;
using AssCS;
using NLog;

namespace Holo;

public class SelectionManager : BindableBase
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private Event _activeEvent;
    private readonly RangeObservableCollection<Event> _selectedEventCollection;

    /// <summary>
    /// The currently-selected event
    /// </summary>
    /// <remarks>
    /// If there are multiple selected events, then <see cref="ActiveEvent"/>
    /// will be the "primary" selected event. <see cref="SelectedEventCollection"/>
    /// will contain the entire selection.
    /// </remarks>
    public Event ActiveEvent
    {
        get => _activeEvent;
        private set => SetProperty(ref _activeEvent, value);
    }

    /// <summary>
    /// Collection of currently-selected events
    /// </summary>
    public ReadOnlyObservableCollection<Event> SelectedEventCollection { get; }

    /// <summary>
    /// Set the active and currently-selected events
    /// </summary>
    /// <param name="active">Event to set as active</param>
    public void Select(Event active)
    {
        Select(active, [active]);
    }

    /// <summary>
    /// Set the active and currently-selected events
    /// </summary>
    /// <param name="active">Event to set as active</param>
    /// <param name="selection">Collection of all selected events</param>
    public void Select(Event active, IList<Event> selection)
    {
        if (active == ActiveEvent && selection.Count == SelectedEventCollection.Count)
            return;

        Logger.Info($"Now selecting {active.Id} ({selection.Count})");
        if (active == ActiveEvent && selection.Count == SelectedEventCollection.Count)
            return;

        ActiveEvent = active;
        _selectedEventCollection.ReplaceRange(selection);
    }

    public SelectionManager(Event initialSelection)
    {
        _activeEvent = initialSelection;
        _selectedEventCollection = [initialSelection];
        SelectedEventCollection = new ReadOnlyObservableCollection<Event>(_selectedEventCollection);
    }
}

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
    /// Denotes if the selection is currently changing
    /// </summary>
    /// <remarks>GUIs can use this property to determine if a change should be reported</remarks>
    public bool IsSelectionChanging { get; private set; } = true;

    /// <summary>
    /// Sets <see cref="IsSelectionChanging"/> to <see langword="true"/>
    /// </summary>
    public void BeginSelectionChange()
    {
        IsSelectionChanging = true;
    }

    /// <summary>
    /// Sets <see cref="IsSelectionChanging"/> to <see langword="false"/>
    /// </summary>
    /// <remarks>
    /// Not called from within the <see cref="SelectionManager"/>.
    /// GUIs can use this method to signal that changes can be reported
    /// </remarks>
    public void EndSelectionChange()
    {
        IsSelectionChanging = false;
        Logger.Debug("Selection change ended");
    }

    /// <summary>
    /// The currently-selected event.
    /// Setting this value sets <see cref="IsSelectionChanging"/> to <see langword="true"/>.
    /// </summary>
    /// <remarks>
    /// If there are multiple selected events, then <see cref="ActiveEvent"/>
    /// will be the "primary" selected event. <see cref="SelectedEventCollection"/>
    /// will contain the entire selection.
    /// </remarks>
    public Event ActiveEvent
    {
        get => _activeEvent;
        private set
        {
            BeginSelectionChange();
            SetProperty(ref _activeEvent, value);
        }
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
    /// Set the active and currently-selected events.
    /// Calling this method sets <see cref="IsSelectionChanging"/> to <see langword="true"/>.
    /// </summary>
    /// <param name="active">Event to set as active</param>
    /// <param name="selection">Collection of all selected events</param>
    public void Select(Event active, IList<Event> selection)
    {
        if (active == ActiveEvent && selection.Count == SelectedEventCollection.Count)
            return;

        Logger.Debug($"Now selecting {active.Id} ({selection.Count})");
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

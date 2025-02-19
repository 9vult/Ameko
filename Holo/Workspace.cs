// SPDX-License-Identifier: MPL-2.0

using System.Collections.ObjectModel;
using AssCS;
using AssCS.History;
using NLog;

namespace Holo;

/// <summary>
/// A group of related files for editing, part of a <see cref="Solution"/>
/// </summary>
/// <remarks>
/// A workspace is the related content normally displayed in a single
/// tab in the editor, which includes a <see cref="Document"/>, and optionally
/// supporting files, like video or audio.
/// </remarks>
public class Workspace : BindableBase
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private readonly Document _document;
    private readonly int _id;
    private Uri? _savePath;
    private bool _isSaved;

    private Event _selectedEvent;
    private readonly RangeObservableCollection<Event> _selectedEventCollection;

    /// <summary>
    /// The ass document in the workspace
    /// </summary>
    public Document Document => _document;

    /// <summary>
    /// The ID of the workspace
    /// </summary>
    public int Id => _id;

    /// <summary>
    /// The path the <see cref="Document"/> is saved to,
    /// or <see langword="null"/> if the document has not been saved.
    /// </summary>
    public Uri? SavePath
    {
        get => _savePath;
        set => SetProperty(ref _savePath, value);
    }

    /// <summary>
    /// <see langword="true"/> if the <see cref="Document"/> has been saved and is up to date
    /// </summary>
    public bool IsSaved
    {
        get => _isSaved;
        private set => SetProperty(ref _isSaved, value);
    }

    /// <summary>
    /// Title to display in the GUI
    /// </summary>
    /// <remarks>Title is prefixed with <c>*</c> if there are unsaved changes</remarks>
    public string Title =>
        SavePath is not null
            ? $"{(IsSaved ? '*' : string.Empty)}{Path.GetFileNameWithoutExtension(SavePath.LocalPath)}"
            : $"New {Id}";

    /// <summary>
    /// The currently-selected event
    /// </summary>
    /// <remarks>
    /// If there are multiple selected events, then <see cref="SelectedEvent"/>
    /// will be the "primary" selected event. <see cref="SelectedEventCollection"/>
    /// will contain the entire selection.
    /// </remarks>
    public Event SelectedEvent
    {
        get => _selectedEvent;
        private set => SetProperty(ref _selectedEvent, value);
    }

    /// <summary>
    /// Collection of currently-selected events
    /// </summary>
    public ReadOnlyObservableCollection<Event> SelectedEventCollection { get; }

    /// <summary>
    /// Set the current selection
    /// </summary>
    /// <param name="primary">"Primary" selection (<see cref="SelectedEvent"/>)</param>
    /// <param name="changeType">Type of change resulting in selection update</param>
    public void SetSelection(Event primary, CommitType changeType)
    {
        SetSelection(primary, [primary], changeType);
    }

    /// <summary>
    /// Set the current selection
    /// </summary>
    /// <param name="primary">"Primary" selection (<see cref="SelectedEvent"/>)</param>
    /// <param name="selection">Entire selection (<see cref="SelectedEventCollection"/>)</param>
    /// <param name="changeType">Type of change resulting in selection update</param>
    public void SetSelection(Event primary, IList<Event> selection, CommitType changeType)
    {
        // See: SubsEditBox::SetSelectedRows
        // https://github.com/arch1t3cht/Aegisub/blob/b2a0b098215d7028ba26f1bf728731fc585f2b99/src/subs_edit_box.cpp#L476
        Logger.Trace($"Setting selection to {primary.Id} (total: {selection.Count})");

        bool amend =
            _document.HistoryManager.CanUndo
            && _document.HistoryManager.LastCommitType == changeType
            && _document.HistoryManager.LastCommitTime.AddSeconds(30) < DateTimeOffset.Now; // TODO: Add an option for this

        // TODO: Determine how to best include descriptions here
        foreach (var e in _selectedEventCollection)
        {
            var parent = _document.EventManager.GetBefore(e.Id);
            _document.HistoryManager.Commit("", changeType, e, parent?.Id, amend);
            amend = true;
        }

        SelectedEvent = primary;
        _selectedEventCollection.ReplaceRange(selection);
    }

    /// <summary>
    /// A group of related files for editing, part of a <see cref="Solution"/>
    /// </summary>
    /// <param name="document">Ass document</param>
    /// <param name="id">Workspace ID</param>
    /// <param name="savePath">Path the <paramref name="document"/> saves to,
    /// or <see langword="null"/> if unsaved</param>
    public Workspace(Document document, int id, Uri? savePath = null)
    {
        _document = document;
        _id = id;
        _savePath = savePath;
        IsSaved = true;

        _selectedEvent = document.EventManager.Head;
        _selectedEventCollection = [_selectedEvent];
        SelectedEventCollection = new ReadOnlyObservableCollection<Event>(_selectedEventCollection);
    }
}

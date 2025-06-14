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
    private readonly SelectionManager _selectionManager;
    private Uri? _savePath;
    private bool _isSaved;

    /// <summary>
    /// The ass document in the workspace
    /// </summary>
    public Document Document => _document;

    /// <summary>
    /// The ID of the workspace
    /// </summary>
    public int Id => _id;

    public SelectionManager SelectionManager => _selectionManager;

    /// <summary>
    /// The path the <see cref="Document"/> is saved to,
    /// or <see langword="null"/> if the document has not been saved.
    /// </summary>
    public Uri? SavePath
    {
        get => _savePath;
        set
        {
            SetProperty(ref _savePath, value);
            RaisePropertyChanged(nameof(DisplayTitle));
        }
    }

    /// <summary>
    /// <see langword="true"/> if the <see cref="Document"/> has been saved and is up to date
    /// </summary>
    public bool IsSaved
    {
        get => _isSaved;
        set
        {
            SetProperty(ref _isSaved, value);
            RaisePropertyChanged(nameof(DisplayTitle));
        }
    }

    /// <summary>
    /// Title of this workspace
    /// </summary>
    public string Title =>
        SavePath is not null ? Path.GetFileNameWithoutExtension(SavePath.LocalPath) : $"New {Id}";

    /// <summary>
    /// Title to display in the GUI
    /// </summary>
    /// <remarks>Title is prefixed with <c>*</c> if there are unsaved changes</remarks>
    public string DisplayTitle =>
        SavePath is not null
            ? $"{(!IsSaved ? '*' : string.Empty)}{Path.GetFileNameWithoutExtension(SavePath.LocalPath)}"
            : $"{(!IsSaved ? '*' : string.Empty)}New {Id}";

    // TODO: May need to raise property changed here - Or move to EventManager under a different name
    /// <summary>
    /// Whether the Actors column in the events grid should be displayed
    /// </summary>
    public bool DisplayActorsColumn => _document.EventManager.Actors.Count > 0;

    /// <summary>
    /// Whether the Effects column in the events grid should be displayed
    /// </summary>
    public bool DisplayEffectsColumn => _document.EventManager.Effects.Count > 0;

    /// <summary>
    /// Commit a change
    /// </summary>
    /// <param name="active">Active event (<see cref="SelectionManager.ActiveEvent"/>)</param>
    /// <param name="changeType">Type of change</param>
    public void Commit(Event active, CommitType changeType)
    {
        Commit([active], changeType);
    }

    /// <summary>
    /// Commit a change
    /// </summary>
    /// <param name="selection">Entire selection (<see cref="SelectionManager.SelectedEventCollection"/>)</param>
    /// <param name="changeType">Type of change</param>
    public void Commit(IList<Event> selection, CommitType changeType)
    {
        // See: SubsEditBox::SetSelectedRows
        // https://github.com/arch1t3cht/Aegisub/blob/b2a0b098215d7028ba26f1bf728731fc585f2b99/src/subs_edit_box.cpp#L476

        var amend =
            _document.HistoryManager.CanUndo
            && _document.HistoryManager.LastCommitType == changeType
            && _document.HistoryManager.LastCommitTime.AddSeconds(30) > DateTimeOffset.Now; // TODO: Add an option for this

        Logger.Trace(
            $"Commiting {selection.Count} events under change {changeType} (amend={amend})"
        );

        // TODO: Determine how to best include descriptions here
        foreach (var e in selection)
        {
            var parent = _document.EventManager.GetBefore(e.Id);
            _document.HistoryManager.Commit("", changeType, e, parent?.Id, amend);
            amend = true;
        }
        IsSaved = false;
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

        _selectionManager = new SelectionManager(Document.EventManager.Head);
    }
}

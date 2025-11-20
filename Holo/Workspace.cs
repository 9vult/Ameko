// SPDX-License-Identifier: MPL-2.0

using AssCS;
using AssCS.History;
using Holo.Configuration;
using Holo.Media.Providers;
using Holo.Providers;
using Microsoft.Extensions.Logging;

namespace Holo;

/// <summary>
/// A group of related files for editing, part of a <see cref="Project"/>
/// </summary>
/// <remarks>
/// A workspace is the related content normally displayed in a single
/// tab in the editor, which includes a <see cref="Document"/>, and optionally
/// supporting files, like video or audio.
/// </remarks>
public class Workspace : BindableBase
{
    private readonly ILogger _logger;
    private FileSystemWatcher? _fileSystemWatcher;
    private DateTimeOffset? _lastWriteTime;
    private DateTimeOffset? _lastExternalModificationAlertTime;

    private Uri? _savePath;

    /// <summary>
    /// The ass document in the workspace
    /// </summary>
    public Document Document { get; }

    /// <summary>
    /// The ID of the workspace
    /// </summary>
    public int Id { get; }

    /// <summary>
    /// Manages selections within the workspace
    /// </summary>
    public SelectionManager SelectionManager { get; }

    /// <summary>
    /// Manages audio/video
    /// </summary>
    public MediaController MediaController { get; }

    /// <summary>
    /// Manages the closed captions attached to the workspace
    /// </summary>
    public ReferenceFileManager ReferenceFileManager { get; }

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

            if (_fileSystemWatcher is null && value is not null)
            {
                _fileSystemWatcher = new FileSystemWatcher(
                    Path.GetDirectoryName(value.LocalPath) ?? "/",
                    Path.GetFileName(value.LocalPath)
                );
                _fileSystemWatcher.NotifyFilter =
                    NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.FileName;
                _fileSystemWatcher.EnableRaisingEvents = true;
                _fileSystemWatcher.Changed += FileSystemWatcherOnChanged;
                _fileSystemWatcher.Renamed += FileSystemWatcherOnChanged;
            }
        }
    }

    /// <summary>
    /// <see langword="true"/> if the <see cref="Document"/> has been saved and is up to date
    /// </summary>
    public bool IsSaved
    {
        get;
        set
        {
            SetProperty(ref field, value);
            RaisePropertyChanged(nameof(DisplayTitle));
            if (value)
                _lastWriteTime = DateTimeOffset.Now;
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

    /// <summary>
    /// Whether the Actors column in the events grid should be displayed
    /// </summary>
    public bool DisplayActorsColumn => Document.EventManager.Actors.Count > 0;

    /// <summary>
    /// Whether the Effects column in the events grid should be displayed
    /// </summary>
    public bool DisplayEffectsColumn => Document.EventManager.Effects.Count > 0;

    /// <summary>
    /// Commit a change
    /// </summary>
    /// <param name="active">Active event (<see cref="SelectionManager.ActiveEvent"/>)</param>
    /// <param name="changeType">Type of change</param>
    /// <param name="amend">Force amend</param>
    public void Commit(Event active, ChangeType changeType, bool amend = false)
    {
        Commit([active], changeType, amend);
    }

    /// <summary>
    /// Commit a change
    /// </summary>
    /// <param name="selection">Entire selection (<see cref="SelectionManager.SelectedEventCollection"/>)</param>
    /// <param name="changeType">Type of change</param>
    /// <param name="amend">Force amend</param>
    public void Commit(IList<Event> selection, ChangeType changeType, bool amend = false)
    {
        // See: SubsEditBox::SetSelectedRows
        // https://github.com/arch1t3cht/Aegisub/blob/b2a0b098215d7028ba26f1bf728731fc585f2b99/src/subs_edit_box.cpp#L476

        amend =
            Document.HistoryManager.CanUndo
            && (
                amend
                || (
                    Document.HistoryManager.LastCommitType == changeType
                    && Document.HistoryManager.LastCommitTime.AddSeconds(30) > DateTimeOffset.Now // TODO: Add an option for this
                    && Document.HistoryManager.PeekHistory().Type == ChangeType.ModifyEvent
                )
            );

        _logger.LogTrace(
            "Commiting {Count} events under change {Type} (amend={Amend})",
            selection.Count,
            changeType,
            amend
        );

        if (changeType == ChangeType.ModifyEvent)
        {
            foreach (var @event in selection)
            {
                Document.HistoryManager.Commit(@event, amend);
                amend = true;
            }
        }
        else
        {
            Document.HistoryManager.Commit(changeType);
        }

        IsSaved = false;
    }

    public void Undo()
    {
        if (!Document.HistoryManager.CanUndo)
            return;
        SelectionManager.BeginSelectionChange();
        _logger.LogTrace("Undoing");
        Document.HistoryManager.Undo();
    }

    public void Redo()
    {
        if (!Document.HistoryManager.CanRedo)
            return;
        SelectionManager.BeginSelectionChange();
        _logger.LogTrace("Redoing");
        Document.HistoryManager.Redo();
    }

    /// <summary>
    /// Propagate changes made to the active event to the other selected events
    /// </summary>
    /// <param name="selection">Selected events</param>
    /// <param name="progenitor">Initial state of the active event</param>
    /// <param name="child">Current state of the active event</param>
    /// <param name="fields">Which fields should be propagated</param>
    private static void PropagateChanges(
        IEnumerable<Event> selection,
        Event progenitor,
        Event child,
        PropagateFields fields
    )
    {
        if (fields == PropagateFields.None)
            return;

        foreach (var e in selection)
        {
            if (progenitor.IsComment != child.IsComment)
                e.IsComment = child.IsComment;
            if (progenitor.Layer != child.Layer)
                e.Layer = child.Layer;
            if (progenitor.Start != child.Start)
                e.Start = child.Start;
            if (progenitor.End != child.End)
                e.End = child.End;
            if (progenitor.Style != child.Style)
                e.Style = child.Style;
            if (progenitor.Actor != child.Actor)
                e.Actor = child.Actor;
            if (progenitor.Effect != child.Effect)
                e.Effect = child.Effect;
            if (progenitor.Margins.Left != child.Margins.Left)
                e.Margins.Left = child.Margins.Left;
            if (progenitor.Margins.Right != child.Margins.Right)
                e.Margins.Right = child.Margins.Right;
            if (progenitor.Margins.Vertical != child.Margins.Vertical)
                e.Margins.Vertical = child.Margins.Vertical;

            if (fields == PropagateFields.NonText)
                continue;

            if (progenitor.Text != child.Text)
                e.Text = child.Text;
        }
    }

    /// <summary>
    /// Listen for updates from the <see cref="_fileSystemWatcher"/>
    /// </summary>
    /// <param name="sender">The FileSystemWatcher</param>
    /// <param name="e"></param>
    /// <exception cref="NotImplementedException"></exception>
    private void FileSystemWatcherOnChanged(object sender, FileSystemEventArgs e)
    {
        if (
            _lastWriteTime is not null
            && DateTimeOffset.Now - _lastWriteTime <= TimeSpan.FromSeconds(5)
        )
            return;
        if (
            _lastExternalModificationAlertTime is not null
            && DateTimeOffset.Now - _lastExternalModificationAlertTime <= TimeSpan.FromSeconds(15)
        )
            return;
        // Also alert if last write time is null
        _lastExternalModificationAlertTime = DateTimeOffset.Now;
        OnFileModifiedExternally?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// A group of related files for editing, part of a <see cref="Project"/>
    /// </summary>
    /// <param name="document">Ass document</param>
    /// <param name="id">Workspace ID</param>
    /// <param name="savePath">Path the <paramref name="document"/> saves to,
    /// or <see langword="null"/> if unsaved</param>
    public Workspace(Document document, int id, Uri? savePath = null)
    {
        _logger = StaticLoggerFactory.GetLogger<Workspace>();
        Document = document;
        Id = id;
        _savePath = savePath;
        IsSaved = true;

        if (_savePath is not null)
        {
            _fileSystemWatcher = new FileSystemWatcher(
                Path.GetDirectoryName(_savePath.LocalPath) ?? "/",
                Path.GetFileName(_savePath.LocalPath)
            );
            _fileSystemWatcher.NotifyFilter =
                NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.FileName;
            _fileSystemWatcher.Changed += FileSystemWatcherOnChanged;
            _fileSystemWatcher.Renamed += FileSystemWatcherOnChanged;
            _fileSystemWatcher.EnableRaisingEvents = true;
        }

        SelectionManager = new SelectionManager(Document.EventManager.Head);
        ReferenceFileManager = new ReferenceFileManager(SelectionManager);

        SelectionManager.SelectionChanged += (_, _) =>
        {
            RaisePropertyChanged(nameof(DisplayActorsColumn));
            RaisePropertyChanged(nameof(DisplayEffectsColumn));
        };

        // TODO: make this cleaner
        var mp = new MizukiSourceProvider();
        MediaController = new MediaController(mp, StaticLoggerFactory.GetLogger<MediaController>());

        // TODO: Should this be here or elsewhere?
        document.HistoryManager.OnChangeMade += (_, _) => MediaController.SetSubtitles(document);
    }

    public event EventHandler<EventArgs>? OnFileModifiedExternally;
}

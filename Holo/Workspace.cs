// SPDX-License-Identifier: MPL-2.0

using AssCS;
using AssCS.History;
using Holo.Configuration;
using Holo.Media.Providers;
using NLog;

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
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private FileSystemWatcher? _fileSystemWatcher;
    private DateTimeOffset? _lastWriteTime;

    private Uri? _savePath;
    private bool _isSaved;

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
        get => _isSaved;
        set
        {
            SetProperty(ref _isSaved, value);
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
                    && Document.HistoryManager.PeekHistory() is EventCommit eventCommit
                    && selection.Any(e => e.Id == eventCommit.Deltas.Last().NewEvent?.Id)
                )
            );

        Logger.Trace(
            $"Commiting {selection.Count} events under change {changeType} (amend={amend})"
        );

        foreach (var e in selection)
        {
            var parent = Document.EventManager.GetBefore(e.Id);
            Document.HistoryManager.Commit(changeType, e, parent?.Id, amend);
            amend = true;
        }
        IsSaved = false;
    }

    public void Undo()
    {
        if (!Document.HistoryManager.CanUndo)
            return;
        SelectionManager.BeginSelectionChange();
        Logger.Trace("Undoing");
        var commit = Document.HistoryManager.Undo();

        if (commit is EventCommit eventCommit)
        {
            for (int i = eventCommit.Deltas.Count - 1; i >= 0; i--)
            {
                var delta = eventCommit.Deltas[i];

                switch (delta.Type)
                {
                    case ChangeType.Add:
                        if (delta.NewEvent is not null)
                            Document.EventManager.Remove(delta.NewEvent.Id);
                        else
                            Logger.Warn($"Cannot undo event addition because newEvent is null");
                        break;
                    case ChangeType.Remove:
                        if (delta.OldEvent is not null)
                        {
                            if (delta.ParentId is null)
                                Document.EventManager.AddFirst(delta.OldEvent);
                            else
                                Document.EventManager.AddAfter(
                                    delta.ParentId.Value,
                                    delta.OldEvent
                                );
                        }
                        else
                            Logger.Warn($"Cannot undo event removal because oldEvent is null");
                        break;
                    case ChangeType.Modify:
                        Document.EventManager.ReplaceInPlace(delta.OldEvent!);
                        break;
                }
            }
        }
    }

    public void Redo()
    {
        if (!Document.HistoryManager.CanRedo)
            return;
        SelectionManager.BeginSelectionChange();
        Logger.Trace("Redoing");
        var commit = Document.HistoryManager.Redo();

        if (commit is EventCommit eventCommit)
        {
            for (int i = 0; i <= eventCommit.Deltas.Count - 1; i++)
            {
                var delta = eventCommit.Deltas[i];

                switch (delta.Type)
                {
                    case ChangeType.Add:
                        if (delta.NewEvent is not null)
                        {
                            if (delta.ParentId is null)
                                Document.EventManager.AddFirst(delta.NewEvent);
                            else
                                Document.EventManager.AddAfter(
                                    delta.ParentId.Value,
                                    delta.NewEvent
                                );
                        }
                        else
                            Logger.Warn($"Cannot redo event addition because newEvent is null");

                        break;
                    case ChangeType.Remove:
                        if (delta.OldEvent is not null)
                            Document.EventManager.Remove(delta.OldEvent.Id);
                        else
                            Logger.Warn($"Cannot redo event removal because oldEvent is null");
                        break;
                    case ChangeType.Modify:
                        Document.EventManager.ReplaceInPlace(delta.NewEvent!);
                        break;
                }
            }
        }
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
        // Also alert if last write time is null
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

        Document.HistoryManager.BeginTransaction([Document.EventManager.Head]);

        // TODO: make this cleaner
        var mp = new MizukiSourceProvider();
        mp.Initialize();
        MediaController = new MediaController(mp);

        // TODO: Should this be here or elsewhere?
        document.HistoryManager.OnChangeMade += (_, _) => MediaController.SetSubtitles(document);
    }

    public event EventHandler<EventArgs>? OnFileModifiedExternally;
}

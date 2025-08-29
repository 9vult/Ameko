// SPDX-License-Identifier: MPL-2.0

using AssCS;
using AssCS.History;
using Holo.Media.Providers;
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
            Document.HistoryManager.CanUndo
            && Document.HistoryManager.LastCommitType == changeType
            && Document.HistoryManager.LastCommitTime.AddSeconds(30) > DateTimeOffset.Now; // TODO: Add an option for this

        Logger.Trace(
            $"Commiting {selection.Count} events under change {changeType} (amend={amend})"
        );

        // TODO: Determine how to best include descriptions here
        foreach (var e in selection)
        {
            var parent = Document.EventManager.GetBefore(e.Id);
            Document.HistoryManager.Commit("", changeType, e, parent?.Id, amend);
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
            foreach (var link in eventCommit.Targets)
            {
                switch (link.Type)
                {
                    case CommitType.EventAdd:
                        Document.EventManager.Remove(link.Target.Id);
                        break;
                    case CommitType.EventRemove:
                        if (link.ParentId.HasValue)
                            Document.EventManager.AddAfter(link.ParentId.Value, link.Target);
                        break;
                    case CommitType.EventMeta:
                    case CommitType.EventTime:
                    case CommitType.EventText:
                    case CommitType.EventFull:
                        Document.EventManager.ReplaceInPlace(link.Target);
                        break;
                    default:
                        Logger.Warn($"Unknown event commit type: {link.Type}");
                        break;
                }
            }
        }
        else if (commit is StyleCommit styleCommit)
        {
            switch (styleCommit.Type)
            {
                case CommitType.StyleAdd:
                    Document.StyleManager.Remove(styleCommit.Target.Id);
                    break;
                case CommitType.StyleRemove:
                case CommitType.StyleMeta:
                    Document.StyleManager.AddOrReplace(styleCommit.Target);
                    break;
                default:
                    Logger.Warn($"Unknown style commit type: {styleCommit.Type}");
                    break;
            }
        }
        else
        {
            throw new NotImplementedException();
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
            foreach (var link in eventCommit.Targets)
            {
                switch (link.Type)
                {
                    case CommitType.EventAdd:
                        if (link.ParentId.HasValue)
                            Document.EventManager.AddAfter(link.ParentId.Value, link.Target);
                        break;
                    case CommitType.EventRemove:
                        Document.EventManager.Remove(link.Target.Id);
                        break;
                    case CommitType.EventMeta:
                    case CommitType.EventTime:
                    case CommitType.EventText:
                    case CommitType.EventFull:
                        Document.EventManager.ReplaceInPlace(link.Target);
                        break;
                    default:
                        Logger.Warn($"Unknown event commit type: {link.Type}");
                        break;
                }
            }
        }
        else if (commit is StyleCommit styleCommit)
        {
            switch (styleCommit.Type)
            {
                case CommitType.StyleAdd:
                case CommitType.StyleMeta:
                    Document.StyleManager.AddOrReplace(styleCommit.Target);
                    break;
                case CommitType.StyleRemove:
                    Document.StyleManager.Remove(styleCommit.Target.Id);
                    break;
                default:
                    Logger.Warn($"Unknown style commit type: {styleCommit.Type}");
                    break;
            }
        }
        else
        {
            throw new NotImplementedException();
        }
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
        Document = document;
        Id = id;
        _savePath = savePath;
        IsSaved = true;

        SelectionManager = new SelectionManager(Document.EventManager.Head);
        ReferenceFileManager = new ReferenceFileManager(SelectionManager);

        // TODO: make this cleaner
        var mp = new MizukiSourceProvider();
        mp.Initialize();
        MediaController = new MediaController(mp);

        // TODO: Should this be here or elsewhere?
        document.HistoryManager.OnChangeMade += (_, _) => MediaController.SetSubtitles(document);
    }
}

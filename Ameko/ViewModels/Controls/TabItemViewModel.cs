// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows.Input;
using Ameko.DataModels;
using Ameko.Messages;
using Ameko.Services;
using Ameko.Utilities;
using Ameko.ViewModels.Dialogs;
using AssCS;
using Holo;
using Holo.Configuration;
using Holo.Configuration.Keybinds;
using Holo.Providers;
using ReactiveUI;

namespace Ameko.ViewModels.Controls;

public partial class TabItemViewModel : ViewModelBase
{
    #region Interactions

    public Interaction<TabItemViewModel, string?> CopyEvents { get; }
    public Interaction<TabItemViewModel, string?> CopyPlaintextEvents { get; }
    public Interaction<TabItemViewModel, string?> CutEvents { get; }
    public Interaction<TabItemViewModel, string[]?> PasteEvents { get; }
    public Interaction<
        PasteOverDialogViewModel,
        PasteOverDialogClosedMessage?
    > ShowPasteOverDialog { get; }
    public Interaction<
        FileModifiedDialogViewModel,
        FileModifiedDialogClosedMessage?
    > ShowFileModifiedDialog { get; }
    public Interaction<Event, Unit> ScrollToAndSelectEvent { get; }
    public Interaction<Unit, Uri?> SaveFrameAs { get; }
    public Interaction<string, Unit> CopyFrame { get; }

    #endregion

    #region Commands

    [KeybindTarget("ameko.event.copy", "Ctrl+C", KeybindContext.Grid)]
    public ICommand CopyEventsCommand { get; }

    [KeybindTarget("ameko.event.cut", "Ctrl+X", KeybindContext.Grid)]
    public ICommand CutEventsCommand { get; }

    [KeybindTarget("ameko.event.paste", "Ctrl+V", KeybindContext.Grid)]
    public ICommand PasteEventsCommand { get; }

    [KeybindTarget("ameko.event.pasteOver", "Ctrl+Shift+V", KeybindContext.Grid)]
    public ICommand PasteOverEventsCommand { get; }

    [KeybindTarget("ameko.event.copyPlaintext", "Ctrl+C", KeybindContext.Grid)]
    public ICommand CopyPlaintextEventsCommand { get; }

    [KeybindTarget("ameko.event.duplicate", "Ctrl+D", KeybindContext.Grid)]
    public ICommand DuplicateEventsCommand { get; }

    [KeybindTarget("ameko.event.insertBefore", KeybindContext.Grid)]
    public ICommand InsertEventBeforeCommand { get; }

    [KeybindTarget("ameko.event.insertAfter", KeybindContext.Grid)]
    public ICommand InsertEventAfterCommand { get; }

    [KeybindTarget("ameko.event.merge", KeybindContext.Grid)]
    public ICommand MergeEventsCommand { get; }

    [KeybindTarget("ameko.event.split", KeybindContext.Grid)]
    public ICommand SplitEventsCommand { get; }

    [KeybindTarget("ameko.event.split.keepTimes", KeybindContext.Grid)]
    public ICommand SplitEventsKeepTimesCommand { get; }

    [KeybindTarget("ameko.event.delete", "Shift+Delete", KeybindContext.Grid)]
    public ICommand DeleteEventsCommand { get; }

    // I don't think this needs a binding?
    public ICommand GetOrCreateAfterCommand { get; }
    public ICommand ToggleTagCommand { get; }

    [KeybindTarget("ameko.event.toggleComment", KeybindContext.Grid)]
    public ICommand ToggleCommentCommand { get; }
    public ICommand ExecuteScriptCommand { get; }

    // Video
    [KeybindTarget("ameko.video.play", KeybindContext.Video)]
    public ICommand PlayPauseCommand { get; }

    [KeybindTarget("ameko.video.stop", KeybindContext.Video)]
    public ICommand StopPlayingCommand { get; }

    [KeybindTarget("ameko.video.playSelection", KeybindContext.Video)]
    public ICommand PlaySelectionCommand { get; }

    [KeybindTarget("ameko.video.toggleAutoSeek", KeybindContext.Video)]
    public ICommand ToggleAutoSeekCommand { get; }

    [KeybindTarget("ameko.video.frame.next", KeybindContext.Video)]
    public ICommand NextFrameCommand { get; }

    [KeybindTarget("ameko.video.frame.previous", KeybindContext.Video)]
    public ICommand PreviousFrameCommand { get; }

    [KeybindTarget("ameko.video.boundary.next", KeybindContext.Video)]
    public ICommand NextBoundaryCommand { get; }

    [KeybindTarget("ameko.video.boundary.previous", KeybindContext.Video)]
    public ICommand PreviousBoundaryCommand { get; }

    [KeybindTarget("ameko.video.keyframe.next", KeybindContext.Video)]
    public ICommand NextKeyframeCommand { get; }

    [KeybindTarget("ameko.video.keyframe.previous", KeybindContext.Video)]
    public ICommand PreviousKeyframeCommand { get; }

    [KeybindTarget("ameko.video.active.start", "Ctrl+D1", KeybindContext.Video)]
    public ICommand ActiveStartCommand { get; }

    [KeybindTarget("ameko.video.active.end", "Ctrl+D2", KeybindContext.Video)]
    public ICommand ActiveEndCommand { get; }

    [KeybindTarget("ameko.video.zoom.in", "Ctrl+OemPlus", KeybindContext.Video)]
    public ICommand ZoomInCommand { get; }

    [KeybindTarget("ameko.video.zoom.out", "Ctrl+OemMinus", KeybindContext.Video)]
    public ICommand ZoomOutCommand { get; }

    [KeybindTarget("ameko.video.rotate.clockwise", KeybindContext.Video)]
    public ICommand RotateClockwiseCommand { get; }

    [KeybindTarget("ameko.video.rotate.counterclockwise", KeybindContext.Video)]
    public ICommand RotateCounterclockwiseCommand { get; }

    [KeybindTarget("ameko.reference.shift.forward", KeybindContext.Editor)]
    public ICommand ShiftReferenceForwardCommand { get; }

    [KeybindTarget("ameko.reference.shift.backward", KeybindContext.Editor)]
    public ICommand ShiftReferenceBackwardCommand { get; }

    // Frame Saving & Copying
    [KeybindTarget("ameko.frame.save", KeybindContext.Global)]
    public ICommand SaveFrameCommand { get; }

    [KeybindTarget("ameko.frame.save.video", KeybindContext.Global)]
    public ICommand SaveFrameVideoOnlyCommand { get; }

    [KeybindTarget("ameko.frame.save.subtitles", KeybindContext.Global)]
    public ICommand SaveFrameSubtitlesOnlyCommand { get; }

    [KeybindTarget("ameko.frame.copy", KeybindContext.Global)]
    public ICommand CopyFrameCommand { get; }

    [KeybindTarget("ameko.frame.copy.video", KeybindContext.Global)]
    public ICommand CopyFrameVideoOnlyCommand { get; }

    [KeybindTarget("ameko.frame.copy.subtitles", KeybindContext.Global)]
    public ICommand CopyFrameSubtitlesOnlyCommand { get; }

    #endregion

    private readonly IScriptService _scriptService;
    private readonly IMessageService _messageService;
    private readonly IIoService _ioService;
    private readonly IViewModelFactory _vmFactory;

    public Workspace Workspace { get; }

    public IProjectProvider ProjectProvider { get; }
    public IConfiguration Configuration { get; }
    public IKeybindService KeybindService { get; }
    public ILayoutProvider LayoutProvider { get; }

    public bool IsIndexing
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public double IndexingProgress
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public int EditBoxSelectionStart
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public int EditBoxSelectionEnd
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public TabItemViewModel(
        IProjectProvider projectProvider,
        IConfiguration configuration,
        IKeybindService keybindService,
        IScriptService scriptService,
        ILayoutProvider layoutProvider,
        IMessageService messageService,
        IViewModelFactory vmFactory,
        IIoService ioService,
        Workspace workspace
    )
    {
        #region Interactions
        CopyEvents = new Interaction<TabItemViewModel, string?>();
        CopyPlaintextEvents = new Interaction<TabItemViewModel, string?>();
        CutEvents = new Interaction<TabItemViewModel, string?>();
        PasteEvents = new Interaction<TabItemViewModel, string[]?>();
        ShowPasteOverDialog =
            new Interaction<PasteOverDialogViewModel, PasteOverDialogClosedMessage?>();
        ShowFileModifiedDialog =
            new Interaction<FileModifiedDialogViewModel, FileModifiedDialogClosedMessage?>();
        ScrollToAndSelectEvent = new Interaction<Event, Unit>();
        SaveFrameAs = new Interaction<Unit, Uri?>();
        CopyFrame = new Interaction<string, Unit>();
        #endregion

        #region Commands
        CopyEventsCommand = CreateCopyEventsCommand();
        CutEventsCommand = CreateCutEventsCommand();
        PasteEventsCommand = CreatePasteEventsCommand();
        PasteOverEventsCommand = CreatePasteOverEventsCommand();
        CopyPlaintextEventsCommand = CreateCopyPlaintextEventsCommand();
        DuplicateEventsCommand = CreateDuplicateEventsCommand();
        InsertEventBeforeCommand = CreateInsertEventBeforeCommand();
        InsertEventAfterCommand = CreateInsertEventAfterCommand();
        MergeEventsCommand = CreateMergeEventsCommand();
        SplitEventsCommand = CreateSplitEventsCommand();
        SplitEventsKeepTimesCommand = CreateSplitEventsKeepTimesCommand();
        DeleteEventsCommand = CreateDeleteEventsCommand();
        GetOrCreateAfterCommand = CreateGetOrCreateAfterCommand();
        ToggleTagCommand = CreateToggleTagCommand();
        ToggleCommentCommand = CreateToggleCommentCommand();

        ShiftReferenceForwardCommand = CreateShiftReferenceForwardCommand();
        ShiftReferenceBackwardCommand = CreateShiftReferenceBackwardCommand();

        ExecuteScriptCommand = CreateExecuteScriptCommand();

        // Video
        PlayPauseCommand = CreatePlayPauseCommand();
        StopPlayingCommand = CreateStopPlayingCommand();
        PlaySelectionCommand = CreatePlaySelectionCommand();
        ToggleAutoSeekCommand = CreateToggleAutoseekCommand();
        NextFrameCommand = CreateNextFrameCommand();
        PreviousFrameCommand = CreatePreviousFrameCommand();
        NextBoundaryCommand = CreateNextBoundaryCommand();
        PreviousBoundaryCommand = CreatePreviousBoundaryCommand();
        NextKeyframeCommand = CreateNextKeyframeCommand();
        PreviousKeyframeCommand = CreatePreviousKeyframeCommand();
        ActiveStartCommand = CreateActiveStartCommand();
        ActiveEndCommand = CreateActiveEndCommand();
        ZoomInCommand = CreateZoomInCommand();
        ZoomOutCommand = CreateZoomOutCommand();
        RotateClockwiseCommand = CreateRotateClockwiseCommand();
        RotateCounterclockwiseCommand = CreateRotateCounterclockwiseCommand();

        // Frame Saving & Copying
        SaveFrameCommand = CreateSaveFrameCommand(SaveFrameMode.Full);
        SaveFrameVideoOnlyCommand = CreateSaveFrameCommand(SaveFrameMode.VideoOnly);
        SaveFrameSubtitlesOnlyCommand = CreateSaveFrameCommand(SaveFrameMode.SubtitlesOnly);
        CopyFrameCommand = CreateCopyFrameCommand(SaveFrameMode.Full);
        CopyFrameVideoOnlyCommand = CreateCopyFrameCommand(SaveFrameMode.VideoOnly);
        CopyFrameSubtitlesOnlyCommand = CreateCopyFrameCommand(SaveFrameMode.SubtitlesOnly);

        #endregion

        Workspace = workspace;
        _scriptService = scriptService;
        _messageService = messageService;
        _ioService = ioService;
        ProjectProvider = projectProvider;
        Configuration = configuration;
        KeybindService = keybindService;
        LayoutProvider = layoutProvider;
        _vmFactory = vmFactory;

        Workspace.OnFileModifiedExternally += async (_, _) =>
        {
            var result = await ShowFileModifiedDialog.Handle(
                new FileModifiedDialogViewModel(Workspace.Title)
            );

            if (result is null || result.Result == FileModifiedDialogClosedResult.Ignore)
                return;

            throw new NotImplementedException("FileModifiedDialogClosedResult.SaveAs");
        };
    }
}

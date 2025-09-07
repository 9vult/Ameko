// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows.Input;
using Ameko.Messages;
using Ameko.Services;
using Ameko.ViewModels.Dialogs;
using AssCS;
using Holo;
using Holo.Configuration;
using Holo.Configuration.Keybinds;
using Holo.Providers;
using ReactiveUI;

namespace Ameko.ViewModels.Controls;

[KeybindContext(KeybindContext.Grid)]
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

    #endregion

    #region Commands

    [KeybindTarget("ameko.event.copy", "Ctrl+C")]
    public ICommand CopyEventsCommand { get; }

    [KeybindTarget("ameko.event.cut", "Ctrl+X")]
    public ICommand CutEventsCommand { get; }

    [KeybindTarget("ameko.event.paste", "Ctrl+V")]
    public ICommand PasteEventsCommand { get; }

    [KeybindTarget("ameko.event.pasteOver", "Ctrl+Shift+V")]
    public ICommand PasteOverEventsCommand { get; }

    [KeybindTarget("ameko.event.copyPlaintext", "Ctrl+C")]
    public ICommand CopyPlaintextEventsCommand { get; }

    [KeybindTarget("ameko.event.duplicate", "Ctrl+D")]
    public ICommand DuplicateEventsCommand { get; }

    [KeybindTarget("ameko.event.insertBefore")]
    public ICommand InsertEventBeforeCommand { get; }

    [KeybindTarget("ameko.event.insertAfter")]
    public ICommand InsertEventAfterCommand { get; }

    [KeybindTarget("ameko.event.merge")]
    public ICommand MergeEventsCommand { get; }

    [KeybindTarget("ameko.event.split")]
    public ICommand SplitEventsCommand { get; }

    [KeybindTarget("ameko.event.split.keepTimes")]
    public ICommand SplitEventsKeepTimesCommand { get; }

    [KeybindTarget("ameko.event.delete", "Shift+Delete")]
    public ICommand DeleteEventsCommand { get; }

    // I don't think this needs a binding?
    public ICommand GetOrCreateAfterCommand { get; }
    public ICommand ToggleTagCommand { get; }

    [KeybindTarget("ameko.event.toggleComment")]
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

    [KeybindTarget("ameko.reference.shift.forward", KeybindContext.Editor)]
    public ICommand ShiftReferenceForwardCommand { get; }

    [KeybindTarget("ameko.reference.shift.backward", KeybindContext.Editor)]
    public ICommand ShiftReferenceBackwardCommand { get; }

    #endregion

    private readonly IScriptService _scriptService;
    private readonly IMessageService _messageService;

    private readonly Workspace _workspace;
    private int _editBoxSelectionStart;
    private int _editBoxSelectionEnd;

    public Workspace Workspace => _workspace;
    public IProjectProvider ProjectProvider { get; }
    public IConfiguration Configuration { get; }
    public IKeybindService KeybindService { get; }
    public ILayoutProvider LayoutProvider { get; }

    public int EditBoxSelectionStart
    {
        get => _editBoxSelectionStart;
        set => this.RaiseAndSetIfChanged(ref _editBoxSelectionStart, value);
    }

    public int EditBoxSelectionEnd
    {
        get => _editBoxSelectionEnd;
        set => this.RaiseAndSetIfChanged(ref _editBoxSelectionEnd, value);
    }

    public TabItemViewModel(
        IProjectProvider projectProvider,
        IConfiguration configuration,
        IKeybindService keybindService,
        IScriptService scriptService,
        ILayoutProvider layoutProvider,
        IMessageService messageService,
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

        #endregion

        _workspace = workspace;
        _scriptService = scriptService;
        _messageService = messageService;
        ProjectProvider = projectProvider;
        Configuration = configuration;
        KeybindService = keybindService;
        LayoutProvider = layoutProvider;

        _workspace.OnFileModifiedExternally += async (_, _) =>
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

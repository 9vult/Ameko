// SPDX-License-Identifier: GPL-3.0-only

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
    public Interaction<TabItemViewModel, string?> CutEvents { get; }
    public Interaction<TabItemViewModel, string[]?> PasteEvents { get; }
    public Interaction<
        PasteOverDialogViewModel,
        PasteOverDialogClosedMessage
    > ShowPasteOverDialog { get; }

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

    [KeybindTarget("ameko.event.delete", "Shift+Delete")]
    public ICommand DeleteEventsCommand { get; }

    // I don't think this needs a binding?
    public ICommand GetOrCreateAfterCommand { get; }
    public ICommand ToggleTagCommand { get; }
    public ICommand ExecuteScriptCommand { get; }

    #endregion

    private readonly IScriptService _scriptService;

    private readonly Workspace _workspace;
    private int _editBoxSelectionStart;
    private int _editBoxSelectionEnd;

    public Workspace Workspace => _workspace;
    public ISolutionProvider SolutionProvider { get; }
    public IConfiguration Configuration { get; }
    public KeybindService KeybindService { get; }

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
        ISolutionProvider solutionProvider,
        IConfiguration configuration,
        KeybindService keybindService,
        IScriptService scriptService,
        Workspace workspace
    )
    {
        #region Interactions
        CopyEvents = new Interaction<TabItemViewModel, string?>();
        CutEvents = new Interaction<TabItemViewModel, string?>();
        PasteEvents = new Interaction<TabItemViewModel, string[]?>();
        ShowPasteOverDialog =
            new Interaction<PasteOverDialogViewModel, PasteOverDialogClosedMessage>();
        #endregion

        #region Commands
        CopyEventsCommand = CreateCopyEventsCommand();
        CutEventsCommand = CreateCutEventsCommand();
        PasteEventsCommand = CreatePasteEventsCommand();
        PasteOverEventsCommand = CreatePasteOverEventsCommand();
        DuplicateEventsCommand = CreateDuplicateEventsCommand();
        InsertEventBeforeCommand = CreateInsertEventBeforeCommand();
        InsertEventAfterCommand = CreateInsertEventAfterCommand();
        MergeEventsCommand = CreateMergeEventsCommand();
        SplitEventsCommand = CreateSplitEventsCommand();
        DeleteEventsCommand = CreateDeleteEventsCommand();
        GetOrCreateAfterCommand = CreateGetOrCreateAfterCommand();
        ToggleTagCommand = CreateToggleTagCommand();

        ExecuteScriptCommand = CreateExecuteScriptCommand();
        #endregion

        _workspace = workspace;
        _scriptService = scriptService;
        SolutionProvider = solutionProvider;
        Configuration = configuration;
        KeybindService = keybindService;
    }
}

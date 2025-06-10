// SPDX-License-Identifier: GPL-3.0-only

using System.Windows.Input;
using Holo;
using Holo.Providers;
using ReactiveUI;

namespace Ameko.ViewModels.Controls;

public partial class TabItemViewModel : ViewModelBase
{
    #region Interactions

    public Interaction<TabItemViewModel, string?> CopyEvents { get; }
    public Interaction<TabItemViewModel, string?> CutEvents { get; }
    public Interaction<TabItemViewModel, string[]?> PasteEvents { get; }

    #endregion

    #region Commands

    public ICommand CopyEventsCommand { get; }
    public ICommand CutEventsCommand { get; }
    public ICommand PasteEventsCommand { get; }

    // TODO: public ICommand PasteOverCommand { get; }
    public ICommand DuplicateEventsCommand { get; }
    public ICommand InsertEventBeforeCommand { get; }
    public ICommand InsertEventAfterCommand { get; }
    public ICommand MergeEventsCommand { get; }
    public ICommand SplitEventsCommand { get; }
    public ICommand DeleteEventsCommand { get; }
    public ICommand GetOrCreateAfterCommand { get; }
    public ICommand ToggleTagCommand { get; }

    #endregion

    private readonly Workspace _workspace;
    private int _editBoxSelectionStart;
    private int _editBoxSelectionEnd;

    public Workspace Workspace => _workspace;
    public ISolutionProvider SolutionProvider { get; }
    public IConfiguration Configuration { get; }

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
        Workspace workspace
    )
    {
        #region Interactions
        CopyEvents = new Interaction<TabItemViewModel, string?>();
        CutEvents = new Interaction<TabItemViewModel, string?>();
        PasteEvents = new Interaction<TabItemViewModel, string[]?>();
        #endregion

        #region Commands
        CopyEventsCommand = CreateCopyEventsCommand();
        CutEventsCommand = CreateCutEventsCommand();
        PasteEventsCommand = CreatePasteEventsCommand();
        // TODO: Paste Over
        DuplicateEventsCommand = CreateDuplicateEventsCommand();
        InsertEventBeforeCommand = CreateInsertEventBeforeCommand();
        InsertEventAfterCommand = CreateInsertEventAfterCommand();
        MergeEventsCommand = CreateMergeEventsCommand();
        SplitEventsCommand = CreateSplitEventsCommand();
        DeleteEventsCommand = CreateDeleteEventsCommand();
        GetOrCreateAfterCommand = CreateGetOrCreateAfterCommand();
        ToggleTagCommand = CreateToggleTagCommand();
        #endregion

        _workspace = workspace;
        SolutionProvider = solutionProvider;
        Configuration = configuration;
    }
}

// SPDX-License-Identifier: GPL-3.0-only

using System.Windows.Input;
using Holo;
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
    #endregion

    private string _title;
    private readonly Workspace _workspace;

    public Workspace Workspace => _workspace;

    public string Title
    {
        get => _title;
        set => this.RaiseAndSetIfChanged(ref _title, value);
    }

    public TabItemViewModel(string title, Workspace workspace)
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
        #endregion

        _title = title;
        _workspace = workspace;
    }
}

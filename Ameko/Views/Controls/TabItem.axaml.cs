// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using Ameko.ViewModels.Controls;
using AssCS.History;
using Avalonia;
using Avalonia.Controls;
using Avalonia.ReactiveUI;
using ReactiveUI;

namespace Ameko.Views.Controls;

public partial class TabItem : ReactiveUserControl<TabItemViewModel>
{
    private async Task DoCopyEventsAsync(IInteractionContext<TabItemViewModel, string?> interaction)
    {
        var window = TopLevel.GetTopLevel(this);
        var selection = interaction.Input.Workspace.SelectionManager.SelectedEventCollection;
        if (window is null || selection.Count == 0)
        {
            interaction.SetOutput(string.Empty);
            return;
        }

        var output = string.Join(Environment.NewLine, selection.Select(e => e.AsAss()));
        await window.Clipboard!.SetTextAsync(output);
        interaction.SetOutput(output);
    }

    private async Task DoCutEventsAsync(IInteractionContext<TabItemViewModel, string?> interaction)
    {
        await DoCopyEventsAsync(interaction);

        // Do the cutting part
        var workspace = interaction.Input.Workspace;
        var eventManager = workspace.Document.EventManager;
        var selectionManager = workspace.SelectionManager;

        if (selectionManager.SelectedEventCollection.Count > 1)
        {
            // Remove all but the primary selection
            eventManager.Remove(
                selectionManager
                    .SelectedEventCollection.Where(e => e.Id != selectionManager.ActiveEvent.Id)
                    .Select(e => e.Id)
                    .ToList()
            );
        }
        // Get or create the next event to select
        var nextEvent =
            eventManager.GetBefore(selectionManager.ActiveEvent.Id)
            ?? eventManager.GetOrCreateAfter(selectionManager.ActiveEvent.Id);

        eventManager.Remove(selectionManager.ActiveEvent.Id);
        workspace.Commit(nextEvent, CommitType.EventRemove);
        selectionManager.Select(nextEvent);
    }

    private async Task DoPasteEventsAsync(
        IInteractionContext<TabItemViewModel, string[]?> interaction
    )
    {
        var window = TopLevel.GetTopLevel(this);
        if (window is null)
        {
            interaction.SetOutput([]);
            return;
        }

        var output = await window.Clipboard!.GetTextAsync();
        if (output is null)
        {
            interaction.SetOutput([]);
            return;
        }

        interaction.SetOutput(output.Split(Environment.NewLine));
    }

    public TabItem()
    {
        InitializeComponent();
        List<TabItemViewModel> previousVMs = [];

        this.WhenActivated(
            (CompositeDisposable disposables) =>
            {
                this.GetObservable(ViewModelProperty)
                    .WhereNotNull()
                    .Subscribe(vm =>
                    {
                        // Skip if already subscribed
                        if (previousVMs.Contains(vm))
                            return;
                        previousVMs.Add(vm);

                        vm.CopyEvents.RegisterHandler(DoCopyEventsAsync);
                        vm.CutEvents.RegisterHandler(DoCutEventsAsync);
                        vm.PasteEvents.RegisterHandler(DoPasteEventsAsync);
                        // TODO: Paste Over
                    })
                    .DisposeWith(disposables);
            }
        );
    }
}

// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using Ameko.ViewModels.Controls;
using AssCS;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.ReactiveUI;
using Avalonia.Threading;
using NLog;
using ReactiveUI;

namespace Ameko.Views.Controls;

public partial class TabItemEventsArea : ReactiveUserControl<TabItemViewModel>
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private static readonly List<TabItemViewModel> PreviousVMs = [];

    private void DoScrollToAndSelectEvent(IInteractionContext<Event, Unit> interaction)
    {
        EventsGrid.ScrollIntoView(interaction.Input, null);
        EventsGrid.SelectedItem = interaction.Input;
        interaction.SetOutput(Unit.Default);
    }

    public TabItemEventsArea()
    {
        InitializeComponent();

        this.WhenActivated(disposables =>
        {
            this.GetObservable(ViewModelProperty)
                .WhereNotNull()
                .Subscribe(vm =>
                {
                    // Skip the rest if already subscribed
                    if (PreviousVMs.Contains(vm))
                        return;
                    PreviousVMs.Add(vm);

                    vm.ScrollToAndSelectEvent.RegisterHandler(DoScrollToAndSelectEvent);

                    EventsGrid.AddHandler(
                        DataGrid.SelectionChangedEvent,
                        DataGrid_OnSelectionChanged,
                        RoutingStrategies.Bubble
                    );
                })
                .DisposeWith(disposables);
        });
    }

    private void DataGrid_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        var active = (Event)EventsGrid.SelectedItem;
        if (active is null)
            return;
        var selection = EventsGrid.SelectedItems.Cast<Event>().ToList();
        // Side effect: set SelectionChanging to true to prevent Commit() calls
        ViewModel?.Workspace.SelectionManager.Select(active, selection);
        ViewModel?.Workspace.MediaController.AutoSeekTo(active);

        EventsGrid.ScrollIntoView(active, null);

        // Prepare for edits
        ViewModel?.Workspace.Document.HistoryManager.BeginTransaction(selection);
        Logger.Debug($"Began transaction for {selection.Count} events");

        // Mark the selection change complete (on the UI thread)
        // to re-allow Commit() calls
        Dispatcher.UIThread.Post(
            () =>
            {
                ViewModel?.Workspace.SelectionManager.EndSelectionChange();
            },
            DispatcherPriority.Background
        );
    }
}

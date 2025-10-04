// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using Ameko.ViewModels.Controls;
using AssCS;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.ReactiveUI;
using Avalonia.Threading;
using ReactiveUI;

namespace Ameko.Views.Controls;

public partial class TabItemEventsArea : ReactiveUserControl<TabItemViewModel>
{
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
                    vm.ScrollToAndSelectEvent.RegisterHandler(DoScrollToAndSelectEvent);

                    EventsGrid.AddHandler(
                        DataGrid.SelectionChangedEvent,
                        DataGrid_OnSelectionChanged,
                        RoutingStrategies.Bubble
                    );

                    EventsGrid.AddHandler(
                        DataGrid.DoubleTappedEvent,
                        DataGrid_OnDoubleTapped,
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

    private void DataGrid_OnDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (!ViewModel?.Workspace.MediaController.IsVideoLoaded == true)
            return;

        var active = (Event)EventsGrid.SelectedItem;
        if (active is null)
            return;

        ViewModel?.Workspace.MediaController.SeekTo(active);
    }
}

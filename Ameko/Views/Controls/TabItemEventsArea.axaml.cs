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
using ReactiveUI;

namespace Ameko.Views.Controls;

public partial class TabItemEventsArea : ReactiveUserControl<TabItemViewModel>
{
    private static readonly List<TabItemViewModel> PreviousVMs = [];

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

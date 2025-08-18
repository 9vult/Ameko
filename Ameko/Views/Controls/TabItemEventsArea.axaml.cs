// SPDX-License-Identifier: GPL-3.0-only

using System.Linq;
using Ameko.ViewModels.Controls;
using AssCS;
using Avalonia.Controls;
using Avalonia.ReactiveUI;
using Avalonia.Threading;

namespace Ameko.Views.Controls;

public partial class TabItemEventsArea : ReactiveUserControl<TabItemViewModel>
{
    public TabItemEventsArea()
    {
        InitializeComponent();
    }

    private void DataGrid_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        var active = (Event)EventsGrid.SelectedItem;
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

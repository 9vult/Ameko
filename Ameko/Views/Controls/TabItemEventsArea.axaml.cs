// SPDX-License-Identifier: GPL-3.0-only

using System.Linq;
using Ameko.ViewModels.Controls;
using AssCS;
using Avalonia.Controls;
using Avalonia.ReactiveUI;

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
        ViewModel?.Workspace.SelectionManager.Select(active, selection);
        ViewModel?.Workspace.MediaController.AutoSeekTo(active);
    }
}

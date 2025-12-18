// SPDX-License-Identifier: GPL-3.0-only

using Ameko.ViewModels.Windows;
using Avalonia.Controls;
using Avalonia.Input;
using Holo.Models;
using ReactiveUI.Avalonia;

namespace Ameko.Views.Controls;

public partial class ProjectExplorer : ReactiveUserControl<MainWindowViewModel>
{
    public ProjectExplorer()
    {
        InitializeComponent();
    }

    private void DocumentTreeItem_OnDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (sender is not TextBlock block)
            return;
        if (block.DataContext is null)
            return;

        _ = ViewModel?.TryLoadReferenced((block.DataContext as DocumentItem)!.Id);
    }
}

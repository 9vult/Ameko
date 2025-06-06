// SPDX-License-Identifier: GPL-3.0-only

using Ameko.ViewModels.Windows;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.ReactiveUI;
using Holo.Models;

namespace Ameko.Views.Controls;

public partial class SolutionExplorer : ReactiveUserControl<MainWindowViewModel>
{
    public SolutionExplorer()
    {
        InitializeComponent();
    }

    private void DocumentTreeItem_OnDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (sender is not TextBlock block)
            return;
        if (block.DataContext is null)
            return;

        ViewModel?.TryLoadReferenced((block.DataContext as DocumentItem)!.Id);
    }
}

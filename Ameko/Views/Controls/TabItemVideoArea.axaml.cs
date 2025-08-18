// SPDX-License-Identifier: GPL-3.0-only

using Ameko.ViewModels.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Ameko.Views.Controls;

public partial class TabItemVideoArea : ReactiveUserControl<TabItemViewModel>
{
    public TabItemVideoArea()
    {
        InitializeComponent();
    }

    private void VideoTarget_OnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        if (ViewModel is null)
            return;

        switch (e.KeyModifiers)
        {
            case KeyModifiers.Control:
                if (e.Delta.Y > 0)
                    ViewModel.ZoomInCommand.Execute(null);
                else
                    ViewModel.ZoomOutCommand.Execute(null);
                e.Handled = true;
                break;
            case KeyModifiers.Alt:
                // Only seek if not playing
                var controller = ViewModel.Workspace.MediaController;
                if (!controller.IsPlaying)
                    controller.SeekTo(controller.CurrentFrame - (int)e.Delta.Y); // Down = forwards
                e.Handled = true;
                break;
            default:
                e.Handled = false; // Fall through
                break;
        }
    }
}

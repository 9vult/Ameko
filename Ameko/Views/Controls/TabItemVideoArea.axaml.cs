// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.Reactive.Disposables.Fluent;
using Ameko.ViewModels.Controls;
using Avalonia;
using Avalonia.Input;
using ReactiveUI;
using ReactiveUI.Avalonia;

namespace Ameko.Views.Controls;

public partial class TabItemVideoArea : ReactiveUserControl<TabItemViewModel>
{
    public TabItemVideoArea()
    {
        InitializeComponent();

        this.WhenActivated(disposables =>
        {
            this.GetObservable(ViewModelProperty)
                .WhereNotNull()
                .Subscribe(vm =>
                {
                    SeekBar.DragStarted += (_, _) =>
                    {
                        vm.Workspace.MediaController.Pause();
                    };
                    SeekBar.DragEnded += (_, _) =>
                    {
                        if (vm.Workspace.MediaController.IsPaused)
                            vm.Workspace.MediaController.Resume();
                    };
                })
                .DisposeWith(disposables);
        });
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
                if (!controller.IsVideoPlaying)
                    controller.SeekTo(controller.CurrentFrame - (int)e.Delta.Y); // Down = forwards
                e.Handled = true;
                break;
            default:
                e.Handled = false; // Fall through
                break;
        }
    }
}

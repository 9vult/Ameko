// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using Ameko.Renderers;
using Ameko.ViewModels.Controls;
using Avalonia;
using Avalonia.Input;
using Avalonia.ReactiveUI;
using ReactiveUI;

namespace Ameko.Views.Controls;

public partial class TabItemAudioArea : ReactiveUserControl<TabItemViewModel>
{
    public TabItemAudioArea()
    {
        InitializeComponent();
        this.WhenActivated(disposables =>
        {
            this.GetObservable(ViewModelProperty)
                .WhereNotNull()
                .Subscribe(vm =>
                {
                    // TODO: Don't do this!!
                    var renderer = new OpenAlAudioRenderer(vm.Workspace.MediaController);
                    renderer.Initialize();
                    vm.Workspace.MediaController.OnPlaybackStart += (_, e) =>
                    {
                        renderer.Play(e.StartTime, e.GoalTime);
                    };
                    vm.Workspace.MediaController.OnPlaybackStop += (_, _) =>
                    {
                        renderer.Stop();
                    };
                })
                .DisposeWith(disposables);
        });
    }

    private void AudioTarget_OnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        if (ViewModel is null)
        {
            e.Handled = true;
            return;
        }
        if (e.Delta.Y > 0)
            ViewModel.Workspace.MediaController.VisualizerPositionMs -= 250; // Quarter second
        if (e.Delta.Y < 0)
            ViewModel.Workspace.MediaController.VisualizerPositionMs += 250;
        e.Handled = true;
    }
}

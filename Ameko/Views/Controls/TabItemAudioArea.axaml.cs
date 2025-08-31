// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using Ameko.Renderers;
using Ameko.ViewModels.Controls;
using Avalonia;
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
}

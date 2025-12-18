// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using Ameko.ViewModels.Dialogs;
using ReactiveUI;
using ReactiveUI.Avalonia;

namespace Ameko.Views.Dialogs;

public partial class SelectTrackDialog : ReactiveWindow<SelectTrackDialogViewModel>
{
    public SelectTrackDialog()
    {
        InitializeComponent();

        this.WhenActivated(disposables =>
        {
            SelectButton.Focus();
            ViewModel?.SelectTrackCommand.Subscribe(Close);

            Disposable.Create(() => { }).DisposeWith(disposables);
        });
    }
}

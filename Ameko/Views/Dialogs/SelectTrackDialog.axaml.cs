// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.Reactive.Disposables;
using Ameko.ViewModels.Dialogs;
using Avalonia.ReactiveUI;
using ReactiveUI;

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

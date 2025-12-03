// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.Reactive.Disposables;
using Ameko.Utilities;
using Ameko.ViewModels.Dialogs;
using Avalonia.Interactivity;
using Avalonia.ReactiveUI;
using ReactiveUI;

namespace Ameko.Views.Dialogs;

public partial class ShiftTimesDialog : ReactiveWindow<ShiftTimesDialogViewModel>
{
    public ShiftTimesDialog()
    {
        InitializeComponent();

        this.WhenActivated(disposables =>
        {
            ViewModel?.ConfirmCommand.Subscribe(Close);
            Disposable.Create(() => { }).DisposeWith(disposables);
        });
    }
}

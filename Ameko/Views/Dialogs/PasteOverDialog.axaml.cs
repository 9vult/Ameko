// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.Reactive.Disposables;
using Ameko.ViewModels.Dialogs;
using Avalonia.ReactiveUI;
using ReactiveUI;

namespace Ameko.Views.Dialogs;

public partial class PasteOverDialog : ReactiveWindow<PasteOverDialogViewModel>
{
    public PasteOverDialog()
    {
        InitializeComponent();

        this.WhenActivated(disposables =>
        {
            ViewModel?.ConfirmCommand.Subscribe(Close);

            Disposable.Create(() => { }).DisposeWith(disposables);
        });
    }
}

// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using Ameko.ViewModels.Dialogs;
using ReactiveUI;
using ReactiveUI.Avalonia;

namespace Ameko.Views.Dialogs;

public partial class SortDialog : ReactiveWindow<SortDialogViewModel>
{
    public SortDialog()
    {
        InitializeComponent();

        this.WhenActivated(disposables =>
        {
            ViewModel?.SortCommand.Subscribe(Close);
            Disposable.Create(() => { }).DisposeWith(disposables);
        });
    }
}

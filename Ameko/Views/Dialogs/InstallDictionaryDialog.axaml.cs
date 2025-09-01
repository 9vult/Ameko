// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.Reactive.Disposables;
using Ameko.ViewModels.Dialogs;
using Avalonia.ReactiveUI;
using ReactiveUI;

namespace Ameko.Views.Dialogs;

public partial class InstallDictionaryDialog : ReactiveWindow<InstallDictionaryDialogViewModel>
{
    public InstallDictionaryDialog()
    {
        InitializeComponent();

        this.WhenActivated(disposables =>
        {
            ViewModel?.IgnoreCommand.Subscribe(Close);
            ViewModel?.DownloadCommand.Subscribe(Close);

            Disposable.Create(() => { }).DisposeWith(disposables);
        });
    }
}

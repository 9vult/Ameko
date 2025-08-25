// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.Reactive.Disposables;
using Ameko.ViewModels.Windows;
using Avalonia.ReactiveUI;
using ReactiveUI;

namespace Ameko.Views.Windows;

public partial class KeybindsWindow : ReactiveWindow<KeybindsWindowViewModel>
{
    public KeybindsWindow()
    {
        InitializeComponent();

        this.WhenActivated(disposables =>
        {
            ViewModel?.SaveCommand.Subscribe(Close);

            Disposable.Create(() => { }).DisposeWith(disposables);
        });
    }
}

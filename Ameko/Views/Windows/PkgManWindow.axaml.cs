// SPDX-License-Identifier: GPL-3.0-only

using System.Reactive.Disposables;
using Ameko.ViewModels.Windows;
using Avalonia.ReactiveUI;
using ReactiveUI;

namespace Ameko.Views.Windows;

public partial class PkgManWindow : ReactiveWindow<PkgManWindowViewModel>
{
    public PkgManWindow()
    {
        InitializeComponent();

        this.WhenActivated(disposables =>
        {
            Disposable.Create(() => { }).DisposeWith(disposables);
        });
    }
}

// SPDX-License-Identifier: GPL-3.0-only

using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using Ameko.ViewModels.Dialogs;
using ReactiveUI;
using ReactiveUI.Avalonia;

namespace Ameko.Views.Dialogs;

public partial class SearchDialog : ReactiveWindow<JumpDialogViewModel>
{
    public SearchDialog()
    {
        InitializeComponent();

        this.WhenActivated(disposables =>
        {
            QueryBox.Focus();
            Disposable.Create(() => { }).DisposeWith(disposables);
        });
    }
}

// SPDX-License-Identifier: GPL-3.0-only

using System.Reactive.Disposables;
using Ameko.ViewModels.Dialogs;
using Avalonia.ReactiveUI;
using ReactiveUI;

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

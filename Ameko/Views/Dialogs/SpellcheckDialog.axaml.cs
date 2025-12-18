// SPDX-License-Identifier: GPL-3.0-only

using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using Ameko.ViewModels.Dialogs;
using ReactiveUI;
using ReactiveUI.Avalonia;

namespace Ameko.Views.Dialogs;

public partial class SpellcheckDialog : ReactiveWindow<JumpDialogViewModel>
{
    public SpellcheckDialog()
    {
        InitializeComponent();

        this.WhenActivated(disposables =>
        {
            MisspellingBox.Focus();
            Disposable.Create(() => { }).DisposeWith(disposables);
        });
    }
}

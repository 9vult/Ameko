// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.Reactive.Disposables;
using Ameko.Utilities;
using Ameko.ViewModels.Dialogs;
using Avalonia.Interactivity;
using Avalonia.ReactiveUI;
using ReactiveUI;

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

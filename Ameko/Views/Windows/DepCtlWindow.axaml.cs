// SPDX-License-Identifier: GPL-3.0-only

using System.Reactive;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using Ameko.ViewModels.Windows;
using Avalonia.ReactiveUI;
using MsBox.Avalonia.Base;
using MsBox.Avalonia.Enums;
using ReactiveUI;

namespace Ameko.Views.Windows;

public partial class DepCtlWindow : ReactiveWindow<DepCtlWindowViewModel>
{
    private async Task DoShowMessageBoxAsync(
        IInteractionContext<IMsBox<ButtonResult>, Unit> interaction
    )
    {
        await interaction.Input.ShowWindowDialogAsync(this);
        interaction.SetOutput(Unit.Default);
    }

    public DepCtlWindow()
    {
        InitializeComponent();

        this.WhenActivated(disposables =>
        {
            if (ViewModel is not null)
            {
                ViewModel.ShowMessageBox.RegisterHandler(DoShowMessageBoxAsync);
            }

            Disposable.Create(() => { }).DisposeWith(disposables);
        });
    }
}

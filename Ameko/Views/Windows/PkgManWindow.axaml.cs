// SPDX-License-Identifier: GPL-3.0-only

using System.Reactive;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using Ameko.ViewModels;
using Ameko.ViewModels.Dialogs;
using Ameko.ViewModels.Windows;
using Ameko.Views.Dialogs;
using Avalonia.Controls;
using Avalonia.ReactiveUI;
using ReactiveUI;

namespace Ameko.Views.Windows;

public partial class PkgManWindow : ReactiveWindow<PkgManWindowViewModel>
{
    /// <summary>
    /// Show an async dialog window
    /// </summary>
    /// <param name="interaction">Interaction</param>
    /// <typeparam name="TDialog">Dialog type</typeparam>
    /// <typeparam name="TViewModel">ViewModel type</typeparam>
    private async Task DoShowDialogAsync<TDialog, TViewModel>(
        IInteractionContext<TViewModel, Unit> interaction
    )
        where TDialog : Window, new()
        where TViewModel : ViewModelBase
    {
        var dialog = new TDialog { DataContext = interaction.Input };
        await dialog.ShowDialog(this);
        interaction.SetOutput(Unit.Default);
    }

    public PkgManWindow()
    {
        InitializeComponent();

        this.WhenActivated(disposables =>
        {
            ViewModel?.ShowChangelog.RegisterHandler(
                DoShowDialogAsync<ChangelogDialog, ChangelogDialogViewModel>
            );

            Disposable.Create(() => { }).DisposeWith(disposables);
        });
    }
}

// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using System.Threading.Tasks;
using Ameko.ViewModels.Dialogs;
using ReactiveUI;
using ReactiveUI.Avalonia;
using Color = AssCS.Color;

namespace Ameko.Views.Dialogs;

public partial class StyleEditorDialog : ReactiveWindow<StyleEditorDialogViewModel>
{
    private async Task DoShowColorDialogAsync(
        IInteractionContext<ColorDialogViewModel, Color?> interaction
    )
    {
        var dialog = new ColorDialog { DataContext = interaction.Input };
        var result = await dialog.ShowDialog<Color?>(this);
        interaction.SetOutput(result);
    }

    public StyleEditorDialog()
    {
        InitializeComponent();

        this.WhenActivated(disposables =>
        {
            if (ViewModel is not null)
            {
                ViewModel.SaveCommand.Subscribe(Close);
                ViewModel.ShowColorDialog.RegisterHandler(DoShowColorDialogAsync);

                Closing += (_, e) =>
                {
                    // Try to commit the style name,
                    // and cancel closing the window if the name is invalid
                    if (!ViewModel.CommitNameChange())
                        e.Cancel = true;
                };
            }

            Disposable.Create(() => { }).DisposeWith(disposables);
        });
    }
}

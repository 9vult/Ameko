// SPDX-License-Identifier: GPL-3.0-only

using System.Reactive.Disposables;
using System.Threading.Tasks;
using Ameko.ViewModels.Dialogs;
using Ameko.ViewModels.Windows;
using Ameko.Views.Dialogs;
using AssCS;
using Avalonia.ReactiveUI;
using ReactiveUI;

namespace Ameko.Views.Windows;

public partial class StyleEditorWindow : ReactiveWindow<StyleEditorWindowViewModel>
{
    private async Task DoShowColorDialogAsync(
        IInteractionContext<ColorDialogViewModel, Color?> interaction
    )
    {
        var dialog = new ColorDialog { DataContext = interaction.Input };
        var result = await dialog.ShowDialog<Color?>(this);
        interaction.SetOutput(result);
    }

    public StyleEditorWindow()
    {
        InitializeComponent();

        this.WhenActivated(disposables =>
        {
            if (ViewModel is not null)
            {
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

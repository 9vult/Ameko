// SPDX-License-Identifier: GPL-3.0-only

using System.Reactive;
using System.Reactive.Disposables;
using Ameko.ViewModels.Dialogs;
using Ameko.ViewModels.Windows;
using Ameko.Views.Dialogs;
using AssCS;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.ReactiveUI;
using ReactiveUI;

namespace Ameko.Views.Windows;

public partial class StylesManagerWindow : ReactiveWindow<StylesManagerWindowViewModel>
{
    private void DoShowStyleEditor(
        IInteractionContext<StyleEditorDialogViewModel, Unit> interaction
    )
    {
        var editor = new StyleEditorDialog { DataContext = interaction.Input };
        editor.ShowDialog(this);
        interaction.SetOutput(Unit.Default);
    }

    private void ListBox_DoubleTapped(object? sender, TappedEventArgs e)
    {
        // TODO: Implement this
        // if (sender is not ListBox listBox)
        //     return;
        // if (listBox.SelectedItem is not Style style)
        //     return;
        //
        // ViewModel?.EditStyleCommand.Execute(style);
    }

    public StylesManagerWindow()
    {
        InitializeComponent();

        this.WhenActivated(disposables =>
        {
            ViewModel?.ShowStyleEditorWindow.RegisterHandler(DoShowStyleEditor);

            Disposable.Create(() => { }).DisposeWith(disposables);
        });
    }
}

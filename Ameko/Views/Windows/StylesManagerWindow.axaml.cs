// SPDX-License-Identifier: GPL-3.0-only

using System.Reactive;
using System.Reactive.Disposables;
using Ameko.ViewModels.Windows;
using AssCS;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.ReactiveUI;
using ReactiveUI;

namespace Ameko.Views.Windows;

public partial class StylesManagerWindow : ReactiveWindow<StylesManagerWindowViewModel>
{
    private static void DoShowStyleEditor(
        IInteractionContext<StyleEditorWindowViewModel, Unit> interaction
    )
    {
        var editor = new StyleEditorWindow { DataContext = interaction.Input };
        editor.Show();
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

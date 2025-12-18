// SPDX-License-Identifier: GPL-3.0-only

using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using System.Threading.Tasks;
using Ameko.Messages;
using Ameko.ViewModels.Dialogs;
using Ameko.ViewModels.Windows;
using Ameko.Views.Dialogs;
using Avalonia.Controls;
using Avalonia.Input;
using ReactiveUI;
using ReactiveUI.Avalonia;

namespace Ameko.Views.Windows;

public partial class StylesManagerWindow : ReactiveWindow<StylesManagerWindowViewModel>
{
    private async Task DoShowStyleEditor(
        IInteractionContext<StyleEditorDialogViewModel, StyleEditorDialogClosedMessage?> interaction
    )
    {
        var editor = new StyleEditorDialog { DataContext = interaction.Input };
        var result = await editor.ShowDialog<StyleEditorDialogClosedMessage?>(this);
        interaction.SetOutput(result);
    }

    private void DocumentStyle_DoubleTapped(object? sender, TappedEventArgs e)
    {
        if (sender is not TextBlock)
            return;
        if (ViewModel?.SelectedDocumentStyle is null)
            return;

        ViewModel?.EditStyleCommand.Execute("document");
    }

    private void ProjectStyle_DoubleTapped(object? sender, TappedEventArgs e)
    {
        if (sender is not TextBlock)
            return;
        if (ViewModel?.SelectedProjectStyle is null)
            return;

        ViewModel?.EditStyleCommand.Execute("project");
    }

    private void GlobalStyle_DoubleTapped(object? sender, TappedEventArgs e)
    {
        if (sender is not TextBlock)
            return;
        if (ViewModel?.SelectedGlobalStyle is null)
            return;

        ViewModel?.EditStyleCommand.Execute("global");
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

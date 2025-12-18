// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.Reactive;
using System.Reactive.Disposables.Fluent;
using System.Threading.Tasks;
using Ameko.ViewModels.Controls;
using AssCS.History;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Interactivity;
using ReactiveUI;
using ReactiveUI.Avalonia;

namespace Ameko.Views.Controls;

public partial class TabItemEditorArea : ReactiveUserControl<TabItemViewModel>
{
    private bool UseSoftLinebreaks =>
        ViewModel?.ProjectProvider.Current.UseSoftLinebreaks
        ?? ViewModel?.Configuration.UseSoftLinebreaks
        ?? false;

    private void EditBox_OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (ViewModel is null)
            return;

        if (e.Key != Key.Enter)
            return;

        e.Handled = true;
        if (e.KeyModifiers.HasFlag(KeyModifiers.Shift))
        {
            var idx = EditBox.CaretIndex;
            if (ViewModel.Workspace.SelectionManager.ActiveEvent.Effect.Contains("code"))
            {
                EditBox.Text = EditBox.Text?.Insert(idx, Environment.NewLine);
                EditBox.CaretIndex += Environment.NewLine.Length;
            }
            else
            {
                EditBox.Text = EditBox.Text?.Insert(idx, UseSoftLinebreaks ? "\\n" : "\\N");
                EditBox.CaretIndex += 2;
            }
        }
        else
        {
            ViewModel.GetOrCreateAfterCommand.Execute(Unit.Default);
            EditBox.Focus();
        }
    }

    // TODO: These don't differentiate between user input and program input
    private void EditBox_OnTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (ViewModel?.Workspace.SelectionManager.IsSelectionChanging == true)
            return;

        ViewModel?.Workspace.Commit(
            ViewModel.Workspace.SelectionManager.SelectedEventCollection,
            ChangeType.ModifyEventText
        );
    }

    private void AnyControl_EventMetaChanged(object? sender, RoutedEventArgs e)
    {
        if (ViewModel?.Workspace.SelectionManager.IsSelectionChanging == true)
            return;

        ViewModel?.Workspace.Commit(
            ViewModel.Workspace.SelectionManager.SelectedEventCollection,
            ChangeType.ModifyEventMeta
        );
    }

    private void AnyControl_EventTimeChanged(object? sender, TextChangedEventArgs e)
    {
        if (ViewModel?.Workspace.SelectionManager.IsSelectionChanging == true)
            return;

        ViewModel?.Workspace.Commit(
            ViewModel.Workspace.SelectionManager.SelectedEventCollection,
            ChangeType.ModifyEventMeta
        );
    }

    public TabItemEditorArea()
    {
        InitializeComponent();

        this.WhenActivated(disposables =>
        {
            this.GetObservable(ViewModelProperty)
                .WhereNotNull()
                .Subscribe(vm =>
                {
                    // TODO: Keybinds

                    EditBox.AddHandler(
                        TextBox.TextChangedEvent,
                        EditBox_OnTextChanged,
                        RoutingStrategies.Bubble
                    );
                    EditBox.AddHandler(KeyDownEvent, EditBox_OnKeyDown, RoutingStrategies.Bubble);
                    EditBox.AddHandler(
                        TextBox.PastingFromClipboardEvent,
                        EditBoxOnPastingFromClipboard,
                        RoutingStrategies.Bubble
                    );
                })
                .DisposeWith(disposables);
        });
    }

    private async Task EditBoxOnPastingFromClipboard(object? sender, RoutedEventArgs e)
    {
        var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
        if (clipboard is not null)
            e.Handled = true;
        else
            return;

        var text = await clipboard.TryGetTextAsync();
        text = text?.Replace(Environment.NewLine, UseSoftLinebreaks ? @"\n" : @"\N");

        if (text is not null && EditBox.Text is not null)
        {
            // No selection → insert at caret
            if (EditBox.SelectionStart == EditBox.SelectionEnd)
            {
                EditBox.Text = EditBox.Text?.Insert(EditBox.CaretIndex, text);
                EditBox.CaretIndex += text.Length;
            }
            // Selection → replace selection
            else
            {
                var start = EditBox.SelectionStart;
                var end = EditBox.SelectionEnd;
                EditBox.Text = EditBox.Text[..start] + text + EditBox.Text[end..];
                EditBox.CaretIndex = start + text.Length;
            }
        }
    }
}

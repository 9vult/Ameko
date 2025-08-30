// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Disposables;
using Ameko.Utilities;
using Ameko.ViewModels.Controls;
using AssCS.History;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.ReactiveUI;
using ReactiveUI;

namespace Ameko.Views.Controls;

public partial class TabItemEditorArea : ReactiveUserControl<TabItemViewModel>
{
    private static readonly List<TabItemViewModel> PreviousVMs = [];

    private bool UseSoftLinebreaks =>
        ViewModel?.SolutionProvider.Current.UseSoftLinebreaks
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
            ChangeType.Modify
        );
    }

    private void AnyControl_EventMetaChanged(object? sender, RoutedEventArgs e)
    {
        if (ViewModel?.Workspace.SelectionManager.IsSelectionChanging == true)
            return;

        ViewModel?.Workspace.Commit(
            ViewModel.Workspace.SelectionManager.SelectedEventCollection,
            ChangeType.Modify
        );
    }

    private void AnyControl_EventTimeChanged(object? sender, TextChangedEventArgs e)
    {
        if (ViewModel?.Workspace.SelectionManager.IsSelectionChanging == true)
            return;

        ViewModel?.Workspace.Commit(
            ViewModel.Workspace.SelectionManager.SelectedEventCollection,
            ChangeType.Modify
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

                    StartBox.AddHandler(
                        TextBox.KeyDownEvent,
                        Extras.PreKeyDownEventHandler,
                        RoutingStrategies.Tunnel
                    );
                    EndBox.AddHandler(
                        TextBox.KeyDownEvent,
                        Extras.PreKeyDownEventHandler,
                        RoutingStrategies.Tunnel
                    );
                    EditBox.AddHandler(
                        TextBox.TextChangedEvent,
                        EditBox_OnTextChanged,
                        RoutingStrategies.Bubble
                    );
                    EditBox.AddHandler(
                        TextBox.KeyDownEvent,
                        EditBox_OnKeyDown,
                        RoutingStrategies.Bubble
                    );

                    if (PreviousVMs.Contains(vm))
                        return;
                    PreviousVMs.Add(vm);
                })
                .DisposeWith(disposables);
        });
    }
}

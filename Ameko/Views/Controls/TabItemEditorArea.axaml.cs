// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Disposables;
using Ameko.Services;
using Ameko.ViewModels.Controls;
using AssCS.History;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.ReactiveUI;
using Holo;
using ReactiveUI;

namespace Ameko.Views.Controls;

public partial class TabItemEditorArea : ReactiveUserControl<TabItemViewModel>
{
    private static bool UseSoftLinebreaks =>
        HoloContext.Instance.Solution.UseSoftLinebreaks
        ?? HoloContext.Instance.Configuration.UseSoftLinebreaks;

    private void EditBox_OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (ViewModel is null)
            return;

        if (e.Key != Key.Enter)
            return;

        if (e.KeyModifiers.HasFlag(KeyModifiers.Shift))
        {
            var idx = EditBox.CaretIndex;
            if (ViewModel.Workspace.SelectionManager.ActiveEvent.Actor.Contains("code"))
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
        ViewModel?.Workspace.Commit(
            ViewModel.Workspace.SelectionManager.SelectedEventCollection,
            CommitType.EventText
        );
    }

    private void AnyControl_EventMetaChanged(object? sender, RoutedEventArgs e)
    {
        ViewModel?.Workspace.Commit(
            ViewModel.Workspace.SelectionManager.SelectedEventCollection,
            CommitType.EventMeta
        );
    }

    private void AnyControl_EventTimeChanged(object? sender, TextChangedEventArgs e)
    {
        ViewModel?.Workspace.Commit(
            ViewModel.Workspace.SelectionManager.SelectedEventCollection,
            CommitType.EventTime
        );
    }

    public TabItemEditorArea()
    {
        InitializeComponent();
        var previousVMs = new List<TabItemViewModel>();

        this.WhenActivated(disposables =>
        {
            this.GetObservable(ViewModelProperty)
                .WhereNotNull()
                .Subscribe(vm =>
                {
                    // TODO: Keybinds

                    if (previousVMs.Contains(vm))
                        return;
                    previousVMs.Add(vm);

                    StartBox.AddHandler(
                        KeyDownEvent,
                        Extras.PreKeyDownEventHandler,
                        RoutingStrategies.Tunnel
                    );
                    EndBox.AddHandler(
                        KeyDownEvent,
                        Extras.PreKeyDownEventHandler,
                        RoutingStrategies.Tunnel
                    );
                })
                .DisposeWith(disposables);
        });
    }
}

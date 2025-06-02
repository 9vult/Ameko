// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.Reactive;
using Ameko.ViewModels.Controls;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.ReactiveUI;
using Holo;

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

    public TabItemEditorArea()
    {
        InitializeComponent();
    }
}

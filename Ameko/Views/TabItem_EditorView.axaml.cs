using Ameko.Services;
using Ameko.ViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.ReactiveUI;
using Holo;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Disposables;

namespace Ameko.Views
{
    public partial class TabItem_EditorView : ReactiveUserControl<TabItemViewModel>
    {
        private List<TabItemViewModel> previousVMs;

        /// <summary>
        /// Check the workspace and the configuration to determine whether to use soft or hard linebreaks
        /// </summary>
        private static bool UseSoftLinebreaks
        {
            get
            {
                if (HoloContext.Instance.Workspace.UseSoftLinebreaks != null)
                    return HoloContext.Instance.Workspace.UseSoftLinebreaks ?? false;
                return HoloContext.Instance.ConfigurationManager?.UseSoftLinebreaks ?? false;
            }
        }

        private void TextBox_KeyDown(object? sender, Avalonia.Input.KeyEventArgs e)
        {
            if (ViewModel == null) return;
            if (e.Key == Avalonia.Input.Key.Enter)
            {
                if (e.KeyModifiers.HasFlag(Avalonia.Input.KeyModifiers.Shift))
                {
                    int idx = editBox.CaretIndex;
                    //if (((Event)eventsGrid.SelectedItem).Effect.Contains("code"))
                    if (ViewModel.SelectedEvent?.Effect.Contains("code") ?? false)
                    {
                        editBox.Text = editBox.Text?.Insert(idx, Environment.NewLine);
                        editBox.CaretIndex += Environment.NewLine.Length;
                    }
                    else
                    {
                        editBox.Text = editBox.Text?.Insert(idx, UseSoftLinebreaks ? "\\n" : "\\N");
                        editBox.CaretIndex += 2;
                    }
                }
                else
                {
                    ViewModel?.NextOrAddEventCommand.Execute(Unit.Default);
                    editBox.Focus();
                }
            }
        }

        private void TextBox_LostFocus(object? sender, RoutedEventArgs e)
        {
            if (ViewModel == null) return;
            ViewModel.FocusLostSelectionEnd = editBox.FocusLostSelectionEnd;
        }

        private void TextBox_TextChanged(object? sender, TextChangedEventArgs e)
        {
            if (ViewModel == null) return;
            ViewModel.UpdateSubtitlesCommand.Execute(Unit.Default);
        }

        private void SetKeybinds()
        {
            editorPanel.KeyBindings.Clear();
            foreach (var pair in HoloContext.Instance.ConfigurationManager.KeybindsRegistry.EditBinds)
            {
                if (pair.Key.StartsWith("ameko")) continue; // Skip builtins
                KeybindService.TrySetKeybind(editorPanel, KeybindContext.EDIT, pair.Key, ViewModel.ActivateScriptCommand, pair.Key);
            }
        }

        private void ConfigurationManager_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(HoloContext.Instance.ConfigurationManager.KeybindsRegistry):
                    SetKeybinds();
                    break;
            }
        }

        public TabItem_EditorView()
        {
            InitializeComponent();
            previousVMs = new List<TabItemViewModel>();

            this.WhenActivated((CompositeDisposable disposables) =>
            {
                this.GetObservable(ViewModelProperty).WhereNotNull()
                .Subscribe(vm =>
                {
                    // Every time
                    SetKeybinds();

                    // Skip the rest if already subscribed
                    if (previousVMs.Contains(vm)) return;
                    previousVMs.Add(vm);

                    startBox.AddHandler(InputElement.KeyDownEvent, Helpers.TimeBox_PreKeyDown, Avalonia.Interactivity.RoutingStrategies.Tunnel);
                    endBox.AddHandler(InputElement.KeyDownEvent, Helpers.TimeBox_PreKeyDown, Avalonia.Interactivity.RoutingStrategies.Tunnel);

                    HoloContext.Instance.ConfigurationManager.PropertyChanged += ConfigurationManager_PropertyChanged;
                });
            });
        }
    }
}

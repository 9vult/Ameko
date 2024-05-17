using Ameko.Services;
using Ameko.ViewModels;
using AssCS;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.ReactiveUI;
using Holo;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;

namespace Ameko.Views
{
    public partial class TabItem_EventsView : ReactiveUserControl<TabItemViewModel>
    {
        private List<TabItemViewModel> previousVMs;

        private void DoScrollIntoView(InteractionContext<Event, Unit> interaction)
        {
            if (interaction == null) return;
            interaction.SetOutput(Unit.Default);
            eventsGrid.SelectedItem = interaction.Input;
            eventsGrid.ScrollIntoView(interaction.Input, null);
        }

        private void SetKeybinds()
        {
            if (ViewModel == null) return;
            // GRID
            eventsGrid.KeyBindings.Clear();
            KeybindService.TrySetKeybind(eventsGrid, KeybindContext.GRID, "ameko.event.duplicate", ViewModel.DuplicateSelectedEventsCommand);
            KeybindService.TrySetKeybind(eventsGrid, KeybindContext.GRID, "ameko.event.copy", ViewModel.CopySelectedEventsCommand);
            KeybindService.TrySetKeybind(eventsGrid, KeybindContext.GRID, "ameko.event.cut", ViewModel.CutSelectedEventsCommand);
            KeybindService.TrySetKeybind(eventsGrid, KeybindContext.GRID, "ameko.event.paste", ViewModel.PasteCommand);
            KeybindService.TrySetKeybind(eventsGrid, KeybindContext.GRID, "ameko.event.pasteover", ViewModel.PasteOverCommand);
            KeybindService.TrySetKeybind(eventsGrid, KeybindContext.GRID, "ameko.event.delete", ViewModel.DeleteSelectedCommand);
            foreach (var pair in HoloContext.Instance.ConfigurationManager.KeybindsRegistry.GridBinds)
            {
                if (pair.Key.StartsWith("ameko")) continue; // Skip builtins
                KeybindService.TrySetKeybind(eventsGrid, KeybindContext.GRID, pair.Key, ViewModel.ActivateScriptCommand, pair.Key);
            }
        }

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

        private void EventsGrid_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            // Cleanup old events
            foreach (Event evnt in e.RemovedItems)
            {
                if (evnt.Text.IndexOf(Environment.NewLine) == -1) continue;

                if (evnt.Effect.Contains("code"))
                {
                    evnt.Text = evnt.TransformCodeToAss();
                }
                else
                {
                    evnt.Text = evnt.Text.Replace(Environment.NewLine, (UseSoftLinebreaks ? "\\n" : "\\N"));
                }
            }

            // Prepare incoming code events
            foreach (Event evnt in e.AddedItems)
            {
                if (!evnt.Effect.Contains("code")) continue;
                evnt.Text = evnt.TransformAssToCode();
            }

            List<Event> list = eventsGrid.SelectedItems.Cast<Event>().ToList();
            Event recent = (Event)eventsGrid.SelectedItem;
            if (ViewModel?.Events?.Count > 0)
            {
                ViewModel?.UpdateEventSelection(list, recent);
                eventsGrid.ScrollIntoView(eventsGrid.SelectedItem, null);
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

        public TabItem_EventsView()
        {
            InitializeComponent();
            previousVMs = new List<TabItemViewModel>();

            this.WhenActivated((CompositeDisposable disposables) =>
            {
                this.GetObservable(ViewModelProperty).WhereNotNull()
                .Subscribe(vm =>
                {
                    // Every time
                    DiscordRPCService.Instance.Set($"{vm.Title}.ass", HoloContext.Instance.Workspace.Name);
                    SetKeybinds();

                    // Skip the rest if already subscribed
                    if (previousVMs.Contains(vm)) return;
                    previousVMs.Add(vm);

                    vm.ScrollIntoViewInteraction.RegisterHandler(DoScrollIntoView);

                    eventsGrid.SelectionChanged += EventsGrid_SelectionChanged;

                    HoloContext.Instance.ConfigurationManager.PropertyChanged += ConfigurationManager_PropertyChanged;
                })
                .DisposeWith(disposables);
            });
        }
    }
}

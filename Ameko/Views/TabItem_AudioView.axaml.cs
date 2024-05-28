using Ameko.ViewModels;
using Avalonia;
using Avalonia.Interactivity;
using Avalonia.ReactiveUI;
using Holo;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Reactive.Disposables;

namespace Ameko.Views
{
    public partial class TabItem_AudioView : ReactiveUserControl<TabItemViewModel>
    {
        private List<TabItemViewModel> previousVMs;

        private void SetKeybinds()
        {
            if (ViewModel == null) return;
            // GRID
            // eventsGrid.KeyBindings.Clear();
            // KeybindService.TrySetKeybind(eventsGrid, KeybindContext.GRID, "ameko.event.duplicate", ViewModel.DuplicateSelectedEventsCommand);

            foreach (var pair in HoloContext.Instance.ConfigurationManager.KeybindsRegistry.AudioBinds)
            {
                // if (pair.Key.StartsWith("ameko")) continue; // Skip builtins
                // KeybindService.TrySetKeybind(eventsGrid, KeybindContext.GRID, pair.Key, ViewModel.ActivateScriptCommand, pair.Key);
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

        public TabItem_AudioView()
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

                    HoloContext.Instance.ConfigurationManager.PropertyChanged += ConfigurationManager_PropertyChanged;
                });
            });
        }
    }
}

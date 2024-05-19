using Ameko.Controls;
using Ameko.Services;
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
    public partial class TabItem_VideoView : ReactiveUserControl<TabItemViewModel>
    {
        private List<TabItemViewModel> previousVMs;

        private void videoTarget_PointerWheelChanged(object? sender, Avalonia.Input.PointerWheelEventArgs e)
        {
            if (ViewModel == null) return;
            ViewModel.ScrollChangeScaleCommand.Execute(e.Delta.Y > 0);
        }

        private void SetKeybinds()
        {
            if (ViewModel == null) return;
            // GRID
            // eventsGrid.KeyBindings.Clear();
            // KeybindService.TrySetKeybind(eventsGrid, KeybindContext.GRID, "ameko.event.duplicate", ViewModel.DuplicateSelectedEventsCommand);
            foreach (var pair in HoloContext.Instance.ConfigurationManager.KeybindsRegistry.VideoBinds)
            {
                //  (pair.Key.StartsWith("ameko")) continue; // Skip builtins
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

        private void SeekSlider_ValueChanged(object? sender, Avalonia.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            if (ViewModel == null) return;
            if (!videoSlider.IsDragging) ViewModel.Wrapper.AVManager.Video.CurrentFrame = (int)e.NewValue;
        }

        private void SeekSlider_DragEnded(object sender, EventArgs e)
        {
            if (ViewModel == null) return;
            ViewModel.Wrapper.AVManager.Video.CurrentFrame = (int)videoSlider.Value;
        }

        public TabItem_VideoView()
        {
            InitializeComponent();
            previousVMs = new List<TabItemViewModel>();

            videoTarget.Children.Add(new SKBitmapRenderer());

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
                })
                .DisposeWith(disposables);
            });
        }
    }
}

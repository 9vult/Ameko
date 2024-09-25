using Ameko.Controls;
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

            switch (e.KeyModifiers)
            {
                case Avalonia.Input.KeyModifiers.Control:
                    ViewModel.ScrollChangeScaleCommand.Execute(e.Delta.Y > 0);
                    e.Handled = true;
                    break;
                case Avalonia.Input.KeyModifiers.Alt:
                case Avalonia.Input.KeyModifiers.Alt | Avalonia.Input.KeyModifiers.Shift:
                    e.Handled = false; // fall through to ScrollViewer
                    break; // TODO: Shift (horizontal) does not seem to be working
                default:
                    if (!ViewModel.Wrapper.AVManager.PlaybackController.IsPlaying) // Only seek if not playing
                        videoSlider.Value += e.Delta.Y; // 1 or -1
                    e.Handled = true;
                    break;
            }
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
            if (!videoSlider.IsDragging) ViewModel.Wrapper.AVManager.PlaybackController.CurrentFrame = (int)e.NewValue;
        }

        private void SeekSlider_DragStarted(object sender, EventArgs e)
        {
            if (ViewModel == null) return;
            if (ViewModel.Wrapper.AVManager.PlaybackController.IsPlaying)
            {
                ViewModel.Wrapper.AVManager.PlaybackController.PausePlaying();
            }
        }

        private void SeekSlider_DragEnded(object sender, EventArgs e)
        {
            if (ViewModel == null) return;
            ViewModel.Wrapper.AVManager.PlaybackController.CurrentFrame = (int)videoSlider.Value;
            if (ViewModel.Wrapper.AVManager.PlaybackController.IsPaused)
            {
                ViewModel.Wrapper.AVManager.PlaybackController.ResumePlaying();
            }
        }

        public TabItem_VideoView()
        {
            InitializeComponent();
            previousVMs = new List<TabItemViewModel>();

            //videoTarget.Children.Add(new SKBitmapRenderer());
            videoTarget.Children.Add(new OpenGlVideoRenderer());

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

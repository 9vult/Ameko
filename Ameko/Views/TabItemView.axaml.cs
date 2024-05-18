using Ameko.Controls;
using Ameko.DataModels;
using Ameko.Services;
using Ameko.ViewModels;
using AssCS;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.ReactiveUI;
using DynamicData;
using Holo;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Threading.Tasks;

namespace Ameko.Views
{
    public partial class TabView : ReactiveUserControl<TabItemViewModel>
    {
        private List<TabItemViewModel> previousVMs;

        private async Task DoCopySelectedEventAsync(InteractionContext<TabItemViewModel, string?> interaction)
        {
            var window = TopLevel.GetTopLevel(this);
            var selected = interaction.Input.Wrapper.SelectedEventCollection;
            if (window == null || selected == null)
            {
                interaction.SetOutput("");
                return;
            }
            var result = string.Join("\n", selected.Select(e => e.AsAss()));
            await window.Clipboard!.SetTextAsync(result);
            interaction.SetOutput(result);
        }

        private async Task DoCutSelectedEventAsync(InteractionContext<TabItemViewModel, string?> interaction)
        {
            var window = TopLevel.GetTopLevel(this);
            var selectedEvents = interaction.Input.Wrapper.SelectedEventCollection;
            var selectedEvent = interaction.Input.Wrapper.SelectedEvent;
            if (window == null || selectedEvents == null || selectedEvent == null)
            {
                interaction.SetOutput("");
                return;
            }
            var result = string.Join("\n", selectedEvents.Select(e => e.AsAss()));
            await window.Clipboard!.SetTextAsync(result);
            interaction.Input.Wrapper.Remove(selectedEvents, selectedEvent);
            interaction.SetOutput(result);
        }

        private async Task DoPasteAsync(InteractionContext<TabItemViewModel, string[]?> interaction)
        {
            var window = TopLevel.GetTopLevel(this);
            if (window == null)
            {
                interaction.SetOutput([]);
                return;
            }
            var result = await window.Clipboard!.GetTextAsync();
            if (result != null)
            {
                interaction.SetOutput(result.Split("\n"));
                return;
            }
            else
                interaction.SetOutput([]);
        }

        private async Task DoShowPasteOverDialogAsync(InteractionContext<PasteOverWindowViewModel, PasteOverField> interaction)
        {
            var window = TopLevel.GetTopLevel(this);
            if (window == null)
            {
                interaction.SetOutput(PasteOverField.None);
                return;
            }
            var dialog = new PasteOverWindow();
            dialog.DataContext = interaction.Input;
            var result = await dialog.ShowDialog<PasteOverField>((Window)window);
            // thonk
            interaction.SetOutput(interaction.Input.Fields);
        }

        private void DoShowStyleEditor(InteractionContext<StyleEditorViewModel, StyleEditorViewModel?> interaction)
        {
            var window = TopLevel.GetTopLevel(this);
            if (window == null)
            {
                interaction.SetOutput(null);
                return;
            }
            var editor = new StyleEditorWindow();
            editor.DataContext = interaction.Input;
            editor.ShowDialog((Window)window);
            interaction.SetOutput(null);
        }

        public TabView()
        {
            InitializeComponent();
            previousVMs = new List<TabItemViewModel>();

            // TODO: TEMP
            var lt = TabLayout.Default;
            ti_GRID.ColumnDefinitions = new ColumnDefinitions(lt.Columns);
            ti_GRID.RowDefinitions = new RowDefinitions(lt.Rows);

            ti_VIDEO.IsVisible = lt.Video;
            ti_VIDEO.SetValue(Grid.ColumnProperty, lt.VideoColumn);
            ti_VIDEO.SetValue(Grid.RowProperty, lt.VideoRow);
            ti_VIDEO.SetValue(Grid.ColumnSpanProperty, lt.VideoColumnSpan);
            ti_VIDEO.SetValue(Grid.RowSpanProperty, lt.VideoRowSpan);

            ti_AUDIO.IsVisible = lt.Audio;
            ti_AUDIO.SetValue(Grid.ColumnProperty, lt.AudioColumn);
            ti_AUDIO.SetValue(Grid.RowProperty, lt.AudioRow);
            ti_AUDIO.SetValue(Grid.ColumnSpanProperty, lt.AudioColumnSpan);
            ti_AUDIO.SetValue(Grid.RowSpanProperty, lt.AudioRowSpan);

            ti_EDITOR.IsVisible = lt.Editor;
            ti_EDITOR.SetValue(Grid.ColumnProperty, lt.EditorColumn);
            ti_EDITOR.SetValue(Grid.RowProperty, lt.EditorRow);
            ti_EDITOR.SetValue(Grid.ColumnSpanProperty, lt.EditorColumnSpan);
            ti_EDITOR.SetValue(Grid.RowSpanProperty, lt.EditorRowSpan);

            ti_EVENTS.IsVisible = lt.Events;
            ti_EVENTS.SetValue(Grid.ColumnProperty, lt.EventsColumn);
            ti_EVENTS.SetValue(Grid.RowProperty, lt.EventsRow);
            ti_EVENTS.SetValue(Grid.ColumnSpanProperty, lt.EventsColumnSpan);
            ti_EVENTS.SetValue(Grid.RowSpanProperty, lt.EventsRowSpan);

            foreach (var split in lt.Splitters)
            {
                GridSplitter splitter = new();
                splitter.ResizeDirection = split.Columns ? GridResizeDirection.Columns : GridResizeDirection.Rows;
                splitter.Background = Brush.Parse("Black");
                splitter.SetValue(Grid.ColumnProperty, split.Column);
                splitter.SetValue(Grid.RowProperty, split.Row);
                splitter.SetValue(Grid.ColumnSpanProperty, split.ColumnSpan);
                splitter.SetValue(Grid.RowSpanProperty, split.RowSpan);
                ti_GRID.Children.Add(splitter);
            }

            // END TEMP

            this.WhenActivated((CompositeDisposable disposables) =>
            {
                this.GetObservable(ViewModelProperty).WhereNotNull()
                .Subscribe(vm =>
                {
                    // Every time
                    DiscordRPCService.Instance.Set($"{vm.Title}.ass", HoloContext.Instance.Workspace.Name);

                    // Skip the rest if already subscribed
                    if (previousVMs.Contains(vm)) return;
                    previousVMs.Add(vm);

                    vm.CopySelectedEvents.RegisterHandler(DoCopySelectedEventAsync);
                    vm.CutSelectedEvents.RegisterHandler(DoCutSelectedEventAsync);
                    vm.Paste.RegisterHandler(DoPasteAsync);
                    vm.ShowPasteOverFieldDialog.RegisterHandler(DoShowPasteOverDialogAsync);
                    vm.ShowStyleEditor.RegisterHandler(DoShowStyleEditor);                    
                })
                .DisposeWith(disposables);
            });
        }
    }
}

using Ameko.Controls;
using Ameko.DataModels;
using Ameko.Services;
using Ameko.ViewModels;
using AssCS;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
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

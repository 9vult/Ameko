// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Ameko.Messages;
using Ameko.ViewModels.Controls;
using Ameko.ViewModels.Dialogs;
using Ameko.Views.Dialogs;
using AssCS;
using AssCS.History;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.ReactiveUI;
using Holo.Configuration.Keybinds;
using Holo.Models;
using ReactiveUI;

namespace Ameko.Views.Controls;

public partial class TabItem : ReactiveUserControl<TabItemViewModel>
{
    private static readonly List<TabItemViewModel> PreviousVMs = [];

    private async Task DoCopyEventsAsync(IInteractionContext<TabItemViewModel, string?> interaction)
    {
        var window = TopLevel.GetTopLevel(this);
        var selection = interaction.Input.Workspace.SelectionManager.SelectedEventCollection;
        if (window is null || selection.Count == 0)
        {
            interaction.SetOutput(string.Empty);
            return;
        }

        var output = string.Join(Environment.NewLine, selection.Select(e => e.AsAss()));
        await window.Clipboard!.SetTextAsync(output);
        interaction.SetOutput(output);
    }

    private async Task DoCutEventsAsync(IInteractionContext<TabItemViewModel, string?> interaction)
    {
        await DoCopyEventsAsync(interaction);

        // Do the cutting part
        var workspace = interaction.Input.Workspace;
        var eventManager = workspace.Document.EventManager;
        var selectionManager = workspace.SelectionManager;

        if (selectionManager.SelectedEventCollection.Count > 1)
        {
            // Remove all but the primary selection
            eventManager.Remove(
                selectionManager
                    .SelectedEventCollection.Where(e => e.Id != selectionManager.ActiveEvent.Id)
                    .Select(e => e.Id)
                    .ToList()
            );
        }
        // Get or create the next event to select
        var nextEvent =
            eventManager.GetBefore(selectionManager.ActiveEvent.Id)
            ?? eventManager.GetOrCreateAfter(selectionManager.ActiveEvent.Id);

        eventManager.Remove(selectionManager.ActiveEvent.Id);
        workspace.Commit(nextEvent, CommitType.EventRemove);
        selectionManager.Select(nextEvent);
    }

    private async Task DoPasteEventsAsync(
        IInteractionContext<TabItemViewModel, string[]?> interaction
    )
    {
        var window = TopLevel.GetTopLevel(this);
        if (window is null)
        {
            interaction.SetOutput([]);
            return;
        }

        var output = await window.Clipboard!.GetTextAsync();
        if (output is null)
        {
            interaction.SetOutput([]);
            return;
        }

        interaction.SetOutput(output.Split(Environment.NewLine));
    }

    private async Task DoShowPasteOverDialogAsync(
        IInteractionContext<PasteOverDialogViewModel, PasteOverDialogClosedMessage> interaction
    )
    {
        var window = TopLevel.GetTopLevel(this);
        if (window is null)
        {
            interaction.SetOutput(new PasteOverDialogClosedMessage(EventField.None));
            return;
        }

        var dialog = new PasteOverDialog { DataContext = interaction.Input };
        var result = await dialog.ShowDialog<PasteOverDialogClosedMessage>((Window)window);
        interaction.SetOutput(result);
    }

    public TabItem()
    {
        InitializeComponent();

        this.WhenActivated(disposables =>
        {
            this.GetObservable(ViewModelProperty)
                .WhereNotNull()
                .Subscribe(vm =>
                {
                    // Alert that the working space has changed
                    MessageBus.Current.SendMessage(new WorkingSpaceChangedMessage());
                    // Listen for scroll messages (?)

                    // Skip if already subscribed
                    if (PreviousVMs.Contains(vm))
                        return;
                    PreviousVMs.Add(vm);

                    vm.CopyEvents.RegisterHandler(DoCopyEventsAsync);
                    vm.CutEvents.RegisterHandler(DoCutEventsAsync);
                    vm.PasteEvents.RegisterHandler(DoPasteEventsAsync);
                    vm.ShowPasteOverDialog.RegisterHandler(DoShowPasteOverDialogAsync);

                    // Register keybinds
                    AttachKeybinds(vm);
                    vm.KeybindService.KeybindRegistrar.OnKeybindsChanged += (_, _) =>
                    {
                        AttachKeybinds(vm);
                    };

                    // Apply layouts
                    ApplyLayout(vm, vm.LayoutProvider.Current);
                    vm.LayoutProvider.OnLayoutChanged += (_, args) =>
                    {
                        ApplyLayout(vm, args.Layout);
                    };
                })
                .DisposeWith(disposables);
        });
    }

    private void ApplyLayout(TabItemViewModel? vm, Layout? layout)
    {
        if (vm is null || layout is null)
            return;

        TabItemGrid.Children.RemoveAll(TabItemGrid.Children.OfType<GridSplitter>());

        TabItemGrid.ColumnDefinitions = new ColumnDefinitions(layout.ColumnDefinitions);
        TabItemGrid.RowDefinitions = new RowDefinitions(layout.RowDefinitions);

        var video = layout.Video;
        TabItemVideoArea.IsVisible = video.IsVisible;
        TabItemVideoArea.SetValue(Grid.ColumnProperty, video.Column);
        TabItemVideoArea.SetValue(Grid.RowProperty, video.Row);
        TabItemVideoArea.SetValue(Grid.ColumnSpanProperty, video.ColumnSpan);
        TabItemVideoArea.SetValue(Grid.RowSpanProperty, video.RowSpan);

        var audio = layout.Audio;
        TabItemAudioArea.IsVisible = audio.IsVisible;
        TabItemAudioArea.SetValue(Grid.ColumnProperty, audio.Column);
        TabItemAudioArea.SetValue(Grid.RowProperty, audio.Row);
        TabItemAudioArea.SetValue(Grid.ColumnSpanProperty, audio.ColumnSpan);
        TabItemAudioArea.SetValue(Grid.RowSpanProperty, audio.RowSpan);

        var editor = layout.Editor;
        TabItemEditorArea.IsVisible = editor.IsVisible;
        TabItemEditorArea.SetValue(Grid.ColumnProperty, editor.Column);
        TabItemEditorArea.SetValue(Grid.RowProperty, editor.Row);
        TabItemEditorArea.SetValue(Grid.ColumnSpanProperty, editor.ColumnSpan);
        TabItemEditorArea.SetValue(Grid.RowSpanProperty, editor.RowSpan);

        var events = layout.Events;
        TabItemEventsArea.IsVisible = events.IsVisible;
        TabItemEventsArea.SetValue(Grid.ColumnProperty, events.Column);
        TabItemEventsArea.SetValue(Grid.RowProperty, events.Row);
        TabItemEventsArea.SetValue(Grid.ColumnSpanProperty, events.ColumnSpan);
        TabItemEventsArea.SetValue(Grid.RowSpanProperty, events.RowSpan);

        foreach (var split in layout.Splitters)
        {
            var splitter = new GridSplitter
            {
                ResizeDirection = split.IsVertical
                    ? GridResizeDirection.Columns
                    : GridResizeDirection.Rows,
                Background = Brushes.Black,
            };
            splitter.SetValue(Grid.ColumnProperty, split.Column);
            splitter.SetValue(Grid.RowProperty, split.Row);
            splitter.SetValue(Grid.ColumnSpanProperty, split.ColumnSpan);
            splitter.SetValue(Grid.RowSpanProperty, split.RowSpan);
            TabItemGrid.Children.Add(splitter);
        }
    }

    private void AttachKeybinds(TabItemViewModel? vm)
    {
        if (vm is null)
            return;
        vm.KeybindService.AttachKeybinds(vm, KeybindContext.Grid, TabItemEventsArea);
        vm.KeybindService.AttachKeybinds(vm, KeybindContext.Editor, TabItemEditorArea);
        vm.KeybindService.AttachKeybinds(vm, KeybindContext.Video, TabItemVideoArea);
        vm.KeybindService.AttachKeybinds(vm, KeybindContext.Audio, TabItemAudioArea);

        vm.KeybindService.AttachScriptKeybinds(
            vm.ExecuteScriptCommand,
            KeybindContext.Grid,
            TabItemEventsArea
        );
        vm.KeybindService.AttachScriptKeybinds(
            vm.ExecuteScriptCommand,
            KeybindContext.Editor,
            TabItemEditorArea
        );
        vm.KeybindService.AttachScriptKeybinds(
            vm.ExecuteScriptCommand,
            KeybindContext.Video,
            TabItemVideoArea
        );
        vm.KeybindService.AttachScriptKeybinds(
            vm.ExecuteScriptCommand,
            KeybindContext.Audio,
            TabItemAudioArea
        );
    }
}

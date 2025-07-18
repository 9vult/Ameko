// SPDX-License-Identifier: GPL-3.0-only

using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using Ameko.ViewModels.Dialogs;
using AssCS;
using AssCS.History;
using ReactiveUI;

namespace Ameko.ViewModels.Controls;

public partial class TabItemViewModel : ViewModelBase
{
    /// <summary>
    /// Command for copying events to the clipboard
    /// </summary>
    private ReactiveCommand<Unit, Unit> CreateCopyEventsCommand()
    {
        return ReactiveCommand.CreateFromTask(async () =>
        {
            await CopyEvents.Handle(this);
        });
    }

    /// <summary>
    /// Command for cutting events to the clipboard
    /// </summary>
    private ReactiveCommand<Unit, Unit> CreateCutEventsCommand()
    {
        return ReactiveCommand.CreateFromTask(async () =>
        {
            await CutEvents.Handle(this);
        });
    }

    /// <summary>
    /// Paste events from the clipboard
    /// </summary>
    private ReactiveCommand<Unit, Unit> CreatePasteEventsCommand()
    {
        return ReactiveCommand.CreateFromTask(async () =>
        {
            var lines = await PasteEvents.Handle(this);
            if (lines is null || lines.Length == 0)
                return;

            var events = new List<Event>();
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                Event @event;
                if (line.StartsWith("Dialogue:") || line.StartsWith("Comment:"))
                {
                    @event = Event.FromAss(Workspace.Document.EventManager.NextId, line.Trim());
                }
                else
                {
                    @event = new Event(Workspace.Document.EventManager.NextId)
                    {
                        Text = line.Trim(),
                    };
                }
                events.Add(@event);
            }

            Workspace.Document.EventManager.AddAfter(
                Workspace.SelectionManager.ActiveEvent.Id,
                events
            );
            Workspace.Commit(events, CommitType.EventAdd);
            Workspace.SelectionManager.Select(events.Last());
        });
    }

    /// <summary>
    /// Paste events from the clipboard over existing events
    /// </summary>
    private ReactiveCommand<Unit, Unit> CreatePasteOverEventsCommand()
    {
        return ReactiveCommand.CreateFromTask(async () =>
        {
            var vm = new PasteOverDialogViewModel();
            var fields = (await ShowPasteOverDialog.Handle(vm)).Fields;

            if (fields == EventField.None)
                return;

            var lines = await PasteEvents.Handle(this);
            if (lines is null || lines.Length == 0)
                return;

            var newEvents = new List<Event>();
            var editedEvents = new List<Event>();

            var i = 0;
            var target = Workspace.SelectionManager.ActiveEvent;
            do
            {
                var line = lines[i++];

                if (string.IsNullOrWhiteSpace(line))
                    continue;

                if (!line.StartsWith("Dialogue:") && !line.StartsWith("Comment:"))
                    continue;

                if (target is not null)
                {
                    var @event = Event.FromAss(-1, line.Trim());
                    target.SetFields(fields, @event);
                    editedEvents.Add(target);
                }
                else
                {
                    target = Event.FromAss(Workspace.Document.EventManager.NextId, line.Trim());
                    Workspace.Document.EventManager.AddLast(target);
                    newEvents.Add(target);
                }
                target = Workspace.Document.EventManager.GetAfter(target.Id);
            } while (i < lines.Length);

            if (editedEvents.Count != 0)
                Workspace.Commit(editedEvents, CommitType.EventFull);
            if (newEvents.Count != 0)
                Workspace.Commit(newEvents, CommitType.EventAdd);
            if (newEvents.Count != 0 && editedEvents.Count != 0)
                Workspace.SelectionManager.Select(
                    newEvents.Concat(editedEvents).OrderBy(e => e.Id).Last()
                );
        });
    }

    /// <summary>
    /// Duplicate selected events
    /// </summary>
    private ReactiveCommand<Unit, Unit> CreateDuplicateEventsCommand()
    {
        return ReactiveCommand.Create(() =>
        {
            var selection = Workspace.SelectionManager.SelectedEventCollection;
            if (selection.Count == 0)
                return;

            var events = selection
                .Select(@event => Workspace.Document.EventManager.Duplicate(@event))
                .ToList();

            Workspace.Commit(events, CommitType.EventAdd);
            Workspace.SelectionManager.Select(events.Last());
        });
    }

    /// <summary>
    /// Insert event before
    /// </summary>
    private ReactiveCommand<Unit, Unit> CreateInsertEventBeforeCommand()
    {
        return ReactiveCommand.Create(() =>
        {
            var @event = Workspace.Document.EventManager.InsertBefore(
                Workspace.SelectionManager.ActiveEvent
            );
            Workspace.Commit(@event, CommitType.EventAdd);
            Workspace.SelectionManager.Select(@event);
        });
    }

    /// <summary>
    /// Insert event after
    /// </summary>
    private ReactiveCommand<Unit, Unit> CreateInsertEventAfterCommand()
    {
        return ReactiveCommand.Create(() =>
        {
            var @event = Workspace.Document.EventManager.InsertAfter(
                Workspace.SelectionManager.ActiveEvent
            );
            Workspace.Commit(@event, CommitType.EventAdd);
            Workspace.SelectionManager.Select(@event);
        });
    }

    /// <summary>
    /// Delete events
    /// </summary>
    private ReactiveCommand<Unit, Unit> CreateDeleteEventsCommand()
    {
        return ReactiveCommand.Create(() =>
        {
            var eventManager = Workspace.Document.EventManager;
            var selectionManager = Workspace.SelectionManager;

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
            Workspace.Commit(nextEvent, CommitType.EventRemove);
            selectionManager.Select(nextEvent);
        });
    }

    /// <summary>
    /// Merge adjacent events
    /// </summary>
    private ReactiveCommand<Unit, Unit> CreateMergeEventsCommand()
    {
        return ReactiveCommand.Create(() =>
        {
            var selectionManager = Workspace.SelectionManager;

            if (selectionManager.SelectedEventCollection.Count != 2)
                return;

            var useSoftBreaks =
                SolutionProvider.Current.UseSoftLinebreaks ?? Configuration.UseSoftLinebreaks;

            var one = selectionManager.SelectedEventCollection[0];
            var two = selectionManager.SelectedEventCollection[1];

            var newEvent = Workspace.Document.EventManager.Merge(one.Id, two.Id, useSoftBreaks);
            if (newEvent is null)
                return;

            Workspace.Commit(newEvent, CommitType.EventAdd | CommitType.EventRemove);
            selectionManager.Select(newEvent);
        });
    }

    /// <summary>
    /// Split events
    /// </summary>
    private ReactiveCommand<Unit, Unit> CreateSplitEventsCommand()
    {
        return ReactiveCommand.Create(() =>
        {
            var selectionManager = Workspace.SelectionManager;

            if (selectionManager.SelectedEventCollection.Count == 0)
                return;

            var newEvents = new List<Event>();
            foreach (var @event in selectionManager.SelectedEventCollection)
            {
                newEvents.AddRange(Workspace.Document.EventManager.Split(@event.Id));
            }

            if (newEvents.Count == 0)
                return;

            Workspace.Commit(newEvents, CommitType.EventAdd | CommitType.EventRemove);
            selectionManager.Select(newEvents.Last());
        });
    }

    /// <summary>
    /// Get or Create the next event in the document
    /// </summary>
    private ReactiveCommand<Unit, Unit> CreateGetOrCreateAfterCommand()
    {
        return ReactiveCommand.Create(() =>
        {
            var initialCount = Workspace.Document.EventManager.Count;
            var nextEvent = Workspace.Document.EventManager.GetOrCreateAfter(
                Workspace.SelectionManager.ActiveEvent.Id
            );
            if (Workspace.Document.EventManager.Count != initialCount)
            {
                Workspace.Commit(nextEvent, CommitType.EventAdd);
            }
            Workspace.SelectionManager.Select(nextEvent);
        });
    }

    /// <summary>
    /// Toggle an ass tag
    /// </summary>
    private ReactiveCommand<string, Unit> CreateToggleTagCommand()
    {
        return ReactiveCommand.Create(
            (string tag) =>
            {
                var @event = Workspace.SelectionManager.ActiveEvent;
                Workspace.Document.StyleManager.TryGet(@event.Style, out var style);

                var shift = @event.ToggleTag(
                    tag,
                    style,
                    EditBoxSelectionStart,
                    EditBoxSelectionEnd
                );
                EditBoxSelectionStart += shift;
                EditBoxSelectionEnd += shift;
            }
        );
    }

    /// <summary>
    /// Toggle whether an event is a comment or not
    /// </summary>
    private ReactiveCommand<Unit, Unit> CreateToggleCommentCommand()
    {
        return ReactiveCommand.Create(() =>
        {
            foreach (var @event in Workspace.SelectionManager.SelectedEventCollection)
            {
                @event.IsComment = !@event.IsComment;
            }
            Workspace.Commit(
                Workspace.SelectionManager.SelectedEventCollection,
                CommitType.EventMeta
            );
        });
    }

    /// <summary>
    /// Execute a Script
    /// </summary>
    private ReactiveCommand<string, Unit> CreateExecuteScriptCommand()
    {
        return ReactiveCommand.CreateFromTask(
            async (string qualifiedName) =>
            {
                await _scriptService.ExecuteScriptAsync(qualifiedName);
            }
        );
    }
}

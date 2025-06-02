// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using AssCS;
using AssCS.History;
using AssCS.Utilities;
using Holo;
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

            Workspace.Document.EventManager.AddAfter(Workspace.SelectedEvent.Id, events);
            Workspace.SetSelection(events.Last(), events, CommitType.EventAdd);
        });
    }

    // TODO: Paste Over command

    /// <summary>
    /// Duplicate selected events
    /// </summary>
    private ReactiveCommand<Unit, Unit> CreateDuplicateEventsCommand()
    {
        return ReactiveCommand.Create(() =>
        {
            var selection = Workspace.SelectedEventCollection;
            if (selection.Count == 0)
                return;

            var events = selection
                .Select(@event => Workspace.Document.EventManager.Duplicate(@event))
                .ToList();

            Workspace.SetSelection(events.Last(), events, CommitType.EventAdd);
        });
    }

    /// <summary>
    /// Insert event before
    /// </summary>
    private ReactiveCommand<Unit, Unit> CreateInsertEventBeforeCommand()
    {
        return ReactiveCommand.Create(() =>
        {
            var @event = Workspace.Document.EventManager.InsertBefore(Workspace.SelectedEvent);
            Workspace.SetSelection(@event, CommitType.EventAdd);
        });
    }

    /// <summary>
    /// Insert event after
    /// </summary>
    private ReactiveCommand<Unit, Unit> CreateInsertEventAfterCommand()
    {
        return ReactiveCommand.Create(() =>
        {
            var @event = Workspace.Document.EventManager.InsertAfter(Workspace.SelectedEvent);
            Workspace.SetSelection(@event, CommitType.EventAdd);
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

            if (Workspace.SelectedEventCollection.Count > 1)
            {
                // Remove all but the primary selection
                eventManager.Remove(
                    Workspace
                        .SelectedEventCollection.Where(e => e.Id != Workspace.SelectedEvent.Id)
                        .Select(e => e.Id)
                        .ToList()
                );
            }
            // Get or create the next event to select
            var nextEvent =
                eventManager.GetBefore(Workspace.SelectedEvent.Id)
                ?? eventManager.GetOrCreateAfter(Workspace.SelectedEvent.Id);

            eventManager.Remove(Workspace.SelectedEvent.Id);
            Workspace.SetSelection(nextEvent, CommitType.EventRemove);
        });
    }

    /// <summary>
    /// Merge adjacent events
    /// </summary>
    private ReactiveCommand<Unit, Unit> CreateMergeEventsCommand()
    {
        return ReactiveCommand.Create(() =>
        {
            if (Workspace.SelectedEventCollection.Count != 2)
                return;

            var useSoftBreaks =
                HoloContext.Instance.Solution.UseSoftLinebreaks
                ?? HoloContext.Instance.Configuration.UseSoftLinebreaks;

            var one = Workspace.SelectedEventCollection[0];
            var two = Workspace.SelectedEventCollection[1];

            var newEvent = Workspace.Document.EventManager.Merge(one.Id, two.Id, useSoftBreaks);
            if (newEvent is not null)
                Workspace.SetSelection(newEvent, CommitType.EventAdd | CommitType.EventRemove);
        });
    }

    /// <summary>
    /// Split events
    /// </summary>
    private ReactiveCommand<Unit, Unit> CreateSplitEventsCommand()
    {
        return ReactiveCommand.Create(() =>
        {
            if (Workspace.SelectedEventCollection.Count == 0)
                return;

            var newEvents = new List<Event>();
            foreach (var @event in Workspace.SelectedEventCollection)
            {
                Workspace.Document.EventManager.Split(@event.Id);
            }

            Workspace.SetSelection(
                newEvents.Last(),
                newEvents,
                CommitType.EventAdd | CommitType.EventRemove
            );
        });
    }
}

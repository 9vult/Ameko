// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using Ameko.ViewModels.Dialogs;
using AssCS;
using AssCS.History;
using Holo.Media;
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
            {
                _messageService.Enqueue(I18N.Other.Message_ClipboardEmpty, TimeSpan.FromSeconds(5));
                return;
            }

            var events = new List<Event>();
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                Event @event;
                if (Event.ValidateAssString(line))
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
            Workspace.Commit(events, ChangeType.Add);
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
            var lines = await PasteEvents.Handle(this);
            if (lines is null || lines.Length == 0)
            {
                _messageService.Enqueue(I18N.Other.Message_ClipboardEmpty, TimeSpan.FromSeconds(5));
                return;
            }

            var vm = new PasteOverDialogViewModel(lines);
            var fields = (await ShowPasteOverDialog.Handle(vm))?.Fields ?? EventField.None;

            if (fields == EventField.None)
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

                if (target is not null)
                {
                    if (Event.ValidateAssString(line))
                    {
                        var @event = Event.FromAss(-1, line.Trim());
                        target.SetFields(fields, @event);
                    }
                    else
                    {
                        target.Text = line.Trim();
                    }

                    editedEvents.Add(target);
                }
                else
                {
                    if (Event.ValidateAssString(line))
                    {
                        target = Event.FromAss(Workspace.Document.EventManager.NextId, line.Trim());
                    }
                    else
                    {
                        target = new Event(Workspace.Document.EventManager.NextId)
                        {
                            Text = line.Trim(),
                        };
                    }
                    Workspace.Document.EventManager.AddLast(target);
                    newEvents.Add(target);
                }
                target = Workspace.Document.EventManager.GetAfter(target.Id);
            } while (i < lines.Length);

            if (editedEvents.Count != 0)
                Workspace.Commit(editedEvents, ChangeType.Modify);
            if (newEvents.Count != 0)
                Workspace.Commit(newEvents, ChangeType.Add, true);
            if (newEvents.Count != 0 && editedEvents.Count != 0)
                Workspace.SelectionManager.Select(
                    newEvents.Concat(editedEvents).OrderBy(e => e.Id).Last()
                );
        });
    }

    /// <summary>
    /// Command for copying plaintext events to the clipboard
    /// </summary>
    private ReactiveCommand<Unit, Unit> CreateCopyPlaintextEventsCommand()
    {
        return ReactiveCommand.CreateFromTask(async () =>
        {
            await CopyPlaintextEvents.Handle(this);
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

            Workspace.Commit(events, ChangeType.Add);
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
            Workspace.Commit(@event, ChangeType.Add);
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
            Workspace.Commit(@event, ChangeType.Add);
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
            var toRemove = selectionManager.ActiveEvent;
            var nextEvent =
                eventManager.GetBefore(toRemove.Id) ?? eventManager.GetAfter(toRemove.Id);

            if (nextEvent is null) // We're deleting the only event, need a new "default event"
            {
                nextEvent = eventManager.GetOrCreateAfter(toRemove.Id);
                nextEvent.Start = Time.FromSeconds(0);
                nextEvent.End = Time.FromSeconds(5);

                eventManager.Remove(toRemove.Id);
                Workspace.Commit(nextEvent, ChangeType.Add);
                Workspace.Commit(toRemove, ChangeType.Remove, true);
            }
            else
            {
                eventManager.Remove(toRemove.Id);
                Workspace.Commit(toRemove, ChangeType.Remove);
            }

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
                ProjectProvider.Current.UseSoftLinebreaks ?? Configuration.UseSoftLinebreaks;

            var one = selectionManager.SelectedEventCollection[0];
            var two = selectionManager.SelectedEventCollection[1];

            var newEvent = Workspace.Document.EventManager.Merge(one.Id, two.Id, useSoftBreaks);
            if (newEvent is null)
                return;

            Workspace.Commit([one, two], ChangeType.Remove);
            Workspace.Commit(newEvent, ChangeType.Add, amend: true);
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

            Workspace.Commit(selectionManager.SelectedEventCollection, ChangeType.Remove);
            Workspace.Commit(newEvents, ChangeType.Add, amend: true);
            selectionManager.Select(newEvents.Last());
        });
    }

    /// <summary>
    /// Split events, keeping times
    /// </summary>
    private ReactiveCommand<Unit, Unit> CreateSplitEventsKeepTimesCommand()
    {
        return ReactiveCommand.Create(() =>
        {
            var selectionManager = Workspace.SelectionManager;

            if (selectionManager.SelectedEventCollection.Count == 0)
                return;

            var newEvents = new List<Event>();
            foreach (var @event in selectionManager.SelectedEventCollection)
            {
                newEvents.AddRange(
                    Workspace.Document.EventManager.Split(@event.Id, keepTimes: true)
                );
            }

            if (newEvents.Count == 0)
                return;

            Workspace.Commit(selectionManager.SelectedEventCollection, ChangeType.Remove);
            Workspace.Commit(newEvents, ChangeType.Add, amend: true);
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
                Workspace.Commit(nextEvent, ChangeType.Add);
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
            Workspace.Commit(Workspace.SelectionManager.SelectedEventCollection, ChangeType.Modify);
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

    //
    // Video
    //

    /// <summary>
    /// Play/Pause video
    /// </summary>
    private ReactiveCommand<Unit, Unit> CreatePlayPauseCommand()
    {
        return ReactiveCommand.Create(() =>
        {
            if (!Workspace.MediaController.IsVideoLoaded)
                return;
            if (Workspace.MediaController.IsPlaying)
                Workspace.MediaController.Pause();
            else
            {
                if (Workspace.MediaController.IsPaused)
                    Workspace.MediaController.Resume();
                else
                    Workspace.MediaController.PlayToEnd();
            }
        });
    }

    /// <summary>
    /// Stop video
    /// </summary>
    private ReactiveCommand<Unit, Unit> CreateStopPlayingCommand()
    {
        return ReactiveCommand.Create(() =>
        {
            if (!Workspace.MediaController.IsVideoLoaded)
                return;
            Workspace.MediaController.Stop();
        });
    }

    /// <summary>
    /// Play selected events
    /// </summary>
    private ReactiveCommand<Unit, Unit> CreatePlaySelectionCommand()
    {
        return ReactiveCommand.Create(() =>
        {
            if (!Workspace.MediaController.IsVideoLoaded)
                return;
            Workspace.MediaController.PlaySelection(
                Workspace.SelectionManager.SelectedEventCollection
            );
        });
    }

    /// <summary>
    /// Toggle Auto-Seek
    /// </summary>
    private ReactiveCommand<Unit, Unit> CreateToggleAutoseekCommand()
    {
        return ReactiveCommand.Create(() =>
        {
            if (!Workspace.MediaController.IsVideoLoaded)
                return;
            Workspace.MediaController.IsAutoSeekEnabled = !Workspace
                .MediaController
                .IsAutoSeekEnabled;
        });
    }

    /// <summary>
    /// Go to next frame
    /// </summary>
    private ReactiveCommand<Unit, Unit> CreateNextFrameCommand()
    {
        return ReactiveCommand.Create(() =>
        {
            if (!Workspace.MediaController.IsVideoLoaded)
                return;
            Workspace.MediaController.SeekTo(Workspace.MediaController.CurrentFrame + 1);
        });
    }

    /// <summary>
    /// Go to previous frame
    /// </summary>
    private ReactiveCommand<Unit, Unit> CreatePreviousFrameCommand()
    {
        return ReactiveCommand.Create(() =>
        {
            if (!Workspace.MediaController.IsVideoLoaded)
                return;
            Workspace.MediaController.SeekTo(Workspace.MediaController.CurrentFrame - 1);
        });
    }

    /// <summary>
    /// Go to next boundary
    /// </summary>
    private ReactiveCommand<Unit, Unit> CreateNextBoundaryCommand()
    {
        return ReactiveCommand.Create(() =>
        {
            if (!Workspace.MediaController.IsVideoLoaded)
                return;
            throw new NotImplementedException();
        });
    }

    /// <summary>
    /// Go to previous boundary
    /// </summary>
    private ReactiveCommand<Unit, Unit> CreatePreviousBoundaryCommand()
    {
        return ReactiveCommand.Create(() =>
        {
            if (!Workspace.MediaController.IsVideoLoaded)
                return;
            throw new NotImplementedException();
        });
    }

    /// <summary>
    /// Go to next keyframe
    /// </summary>
    private ReactiveCommand<Unit, Unit> CreateNextKeyframeCommand()
    {
        return ReactiveCommand.Create(() =>
        {
            if (!Workspace.MediaController.IsVideoLoaded)
                return;
            var nextKeyframe = Workspace.MediaController.VideoInfo?.Keyframes.FirstOrDefault(kf =>
                kf > Workspace.MediaController.CurrentFrame
            );

            if (nextKeyframe is not null)
                Workspace.MediaController.SeekTo(nextKeyframe.Value);
        });
    }

    /// <summary>
    /// Go to previous keyframe
    /// </summary>
    private ReactiveCommand<Unit, Unit> CreatePreviousKeyframeCommand()
    {
        return ReactiveCommand.Create(() =>
        {
            if (!Workspace.MediaController.IsVideoLoaded)
                return;
            var nextKeyframe = Workspace.MediaController.VideoInfo?.Keyframes.LastOrDefault(kf =>
                kf < Workspace.MediaController.CurrentFrame
            );

            if (nextKeyframe is not null)
                Workspace.MediaController.SeekTo(nextKeyframe.Value);
        });
    }

    /// <summary>
    /// Go to start of active event
    /// </summary>
    private ReactiveCommand<Unit, Unit> CreateActiveStartCommand()
    {
        return ReactiveCommand.Create(() =>
        {
            if (!Workspace.MediaController.IsVideoLoaded)
                return;
            var selection = Workspace.SelectionManager.ActiveEvent;
            Workspace.MediaController.SeekTo(selection);
        });
    }

    /// <summary>
    /// Go to end of active event
    /// </summary>
    private ReactiveCommand<Unit, Unit> CreateActiveEndCommand()
    {
        return ReactiveCommand.Create(() =>
        {
            if (!Workspace.MediaController.IsVideoLoaded)
                return;
            var selection = Workspace.SelectionManager.ActiveEvent;
            Workspace.MediaController.SeekToEnd(selection);
        });
    }

    /// <summary>
    /// Increase video zoom level
    /// </summary>
    private ReactiveCommand<Unit, Unit> CreateZoomInCommand()
    {
        return ReactiveCommand.Create(() =>
        {
            if (!Workspace.MediaController.IsVideoLoaded)
                return;
            var scales = ScaleFactor.Scales;
            var index = scales.IndexOf(Workspace.MediaController.ScaleFactor);

            if (index < scales.Count - 1)
                Workspace.MediaController.ScaleFactor = scales[index + 1];
        });
    }

    /// <summary>
    /// Decrease video zoom level
    /// </summary>
    private ReactiveCommand<Unit, Unit> CreateZoomOutCommand()
    {
        return ReactiveCommand.Create(() =>
        {
            if (!Workspace.MediaController.IsVideoLoaded)
                return;
            var scales = ScaleFactor.Scales;
            var index = scales.IndexOf(Workspace.MediaController.ScaleFactor);

            if (index > 0)
                Workspace.MediaController.ScaleFactor = scales[index - 1];
        });
    }

    /// <summary>
    /// Rotate Clockwise
    /// </summary>
    private ReactiveCommand<Unit, Unit> CreateRotateClockwiseCommand()
    {
        return ReactiveCommand.Create(() =>
        {
            if (!Workspace.MediaController.IsVideoLoaded)
                return;
            var angles = RotationalFactor.Angles;
            var index = angles.IndexOf(Workspace.MediaController.RotationalFactor);

            if (index < angles.Count - 1)
                Workspace.MediaController.RotationalFactor = angles[index + 1];
        });
    }

    /// <summary>
    /// Rotate Counterclockwise
    /// </summary>
    private ReactiveCommand<Unit, Unit> CreateRotateCounterclockwiseCommand()
    {
        return ReactiveCommand.Create(() =>
        {
            if (!Workspace.MediaController.IsVideoLoaded)
                return;
            var angles = RotationalFactor.Angles;
            var index = angles.IndexOf(Workspace.MediaController.RotationalFactor);

            if (index > 0)
                Workspace.MediaController.RotationalFactor = angles[index - 1];
        });
    }

    /// <summary>
    /// Shift reference file forwards
    /// </summary>
    private ReactiveCommand<Unit, Unit> CreateShiftReferenceForwardCommand()
    {
        return ReactiveCommand.Create(() =>
        {
            if (!Workspace.ReferenceFileManager.IsReferenceLoaded)
                return;
            Workspace.ReferenceFileManager.Shift(1);
        });
    }

    /// <summary>
    /// Shift reference file forwards
    /// </summary>
    private ReactiveCommand<Unit, Unit> CreateShiftReferenceBackwardCommand()
    {
        return ReactiveCommand.Create(() =>
        {
            if (!Workspace.ReferenceFileManager.IsReferenceLoaded)
                return;
            Workspace.ReferenceFileManager.Shift(-1);
        });
    }
}

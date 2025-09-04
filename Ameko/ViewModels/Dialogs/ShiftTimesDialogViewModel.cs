// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.Linq;
using System.Reactive;
using Ameko.DataModels;
using AssCS;
using AssCS.History;
using Holo;
using Holo.Models;
using ReactiveUI;

namespace Ameko.ViewModels.Dialogs;

public partial class ShiftTimesDialogViewModel : ViewModelBase
{
    private readonly Workspace _workspace;
    private readonly Time _shiftTime;
    private int _shiftFrames;
    private int _shiftMillis;

    private ShiftTimesType _shiftType;
    private ShiftTimesDirection _shiftDirection;
    private ShiftTimesFilter _shiftFilter;
    private ShiftTimesTarget _shiftTarget;

    public ReactiveCommand<Unit, object?> ConfirmCommand { get; }

    public bool CanShiftFrames { get; set; }

    public Time ShiftTime => _shiftTime;

    public int ShiftFrames
    {
        get => _shiftFrames;
        set => this.RaiseAndSetIfChanged(ref _shiftFrames, value);
    }

    public int ShiftMillis
    {
        get => _shiftMillis;
        set => this.RaiseAndSetIfChanged(ref _shiftMillis, value);
    }

    public ShiftTimesType ShiftType
    {
        get => _shiftType;
        set => this.RaiseAndSetIfChanged(ref _shiftType, value);
    }

    public ShiftTimesDirection ShiftDirection
    {
        get => _shiftDirection;
        set => this.RaiseAndSetIfChanged(ref _shiftDirection, value);
    }

    public ShiftTimesFilter ShiftFilter
    {
        get => _shiftFilter;
        set => this.RaiseAndSetIfChanged(ref _shiftFilter, value);
    }

    public ShiftTimesTarget ShiftTarget
    {
        get => _shiftTarget;
        set => this.RaiseAndSetIfChanged(ref _shiftTarget, value);
    }

    private object? ShiftTimes()
    {
        var events = ShiftFilter switch
        {
            ShiftTimesFilter.AllEvents => _workspace.Document.EventManager.Events,
            ShiftTimesFilter.SelectedEvents =>
                _workspace.SelectionManager.SelectedEventCollection.ToList(),
            _ => throw new ArgumentOutOfRangeException(),
        };

        switch (ShiftType)
        {
            case ShiftTimesType.Time:
                foreach (var @event in events)
                {
                    if (ShiftTarget is ShiftTimesTarget.Start or ShiftTimesTarget.Both)
                    {
                        if (ShiftDirection is ShiftTimesDirection.Forward)
                            @event.Start += ShiftTime;
                        else
                            @event.Start -= ShiftTime;
                    }
                    if (ShiftTarget is ShiftTimesTarget.End or ShiftTimesTarget.Both)
                    {
                        if (ShiftDirection is ShiftTimesDirection.Forward)
                            @event.End += ShiftTime;
                        else
                            @event.End -= ShiftTime;
                    }
                }
                break;
            case ShiftTimesType.Frames:
                var vi = _workspace.MediaController.VideoInfo;
                if (vi is null)
                    break;
                foreach (var @event in events)
                {
                    if (ShiftTarget is ShiftTimesTarget.Start or ShiftTimesTarget.Both)
                    {
                        if (ShiftDirection is ShiftTimesDirection.Forward)
                            @event.Start = vi.TimeFromFrame(
                                vi.FrameFromTime(@event.Start) + ShiftFrames
                            );
                        else
                            @event.Start = vi.TimeFromFrame(
                                vi.FrameFromTime(@event.Start) - ShiftFrames
                            );
                    }
                    if (ShiftTarget is ShiftTimesTarget.End or ShiftTimesTarget.Both)
                    {
                        if (ShiftDirection is ShiftTimesDirection.Forward)
                            @event.End = vi.TimeFromFrame(
                                vi.FrameFromTime(@event.End) + ShiftFrames
                            );
                        else
                            @event.End = vi.TimeFromFrame(
                                vi.FrameFromTime(@event.End) - ShiftFrames
                            );
                    }
                }
                break;
            case ShiftTimesType.Milliseconds:
                var msTime = Time.FromMillis(ShiftMillis);
                foreach (var @event in events)
                {
                    if (ShiftTarget is ShiftTimesTarget.Start or ShiftTimesTarget.Both)
                    {
                        if (ShiftDirection is ShiftTimesDirection.Forward)
                            @event.Start += msTime;
                        else
                            @event.Start -= msTime;
                    }
                    if (ShiftTarget is ShiftTimesTarget.End or ShiftTimesTarget.Both)
                    {
                        if (ShiftDirection is ShiftTimesDirection.Forward)
                            @event.End += msTime;
                        else
                            @event.End -= msTime;
                    }
                }
                break;
            default:
                break;
        }

        // Sanity check
        foreach (var @event in events)
        {
            if (ShiftDirection is ShiftTimesDirection.Forward && @event.Start > @event.End)
                @event.End = @event.Start;
            if (ShiftDirection is ShiftTimesDirection.Backward && @event.End < @event.Start)
                @event.Start = @event.End;
        }

        _workspace.Commit(events, ChangeType.Modify);
        return null;
    }

    public ShiftTimesDialogViewModel(Workspace workspace)
    {
        _workspace = workspace;

        _shiftTime = new Time();
        ShiftType = ShiftTimesType.Time;
        ShiftDirection = ShiftTimesDirection.Forward;
        ShiftFilter = ShiftTimesFilter.SelectedEvents;
        ShiftTarget = ShiftTimesTarget.Both;

        CanShiftFrames = workspace.MediaController.IsVideoLoaded;

        ConfirmCommand = ReactiveCommand.Create(ShiftTimes);
    }
}

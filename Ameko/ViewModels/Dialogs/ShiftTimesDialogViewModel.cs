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

    private ShiftTimesType _shiftType;
    private ShiftTimesDirection _shiftDirection;
    private ShiftTimesFilter _shiftFilter;
    private ShiftTimesTarget _shiftTarget;

    public ReactiveCommand<Unit, object?> ConfirmCommand { get; }

    public Time ShiftTime => _shiftTime;

    public int ShiftFrames
    {
        get => _shiftFrames;
        set => this.RaiseAndSetIfChanged(ref _shiftFrames, value);
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
            // TODO: Implement frames
            case ShiftTimesType.Frames:
            default:
                break;
        }

        _workspace.Commit(events, CommitType.EventTime);
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

        ConfirmCommand = ReactiveCommand.Create(ShiftTimes);
    }
}

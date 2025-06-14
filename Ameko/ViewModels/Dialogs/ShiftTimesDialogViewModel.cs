// SPDX-License-Identifier: GPL-3.0-only

using System.Reactive;
using AssCS;
using Holo.Configuration;
using Holo.Models;
using ReactiveUI;

namespace Ameko.ViewModels.Dialogs;

public partial class ShiftTimesDialogViewModel : ViewModelBase
{
    private Time _shiftTime;
    private int _shiftFrames;

    private ShiftTimesType _shiftType;
    private ShiftTimesDirection _shiftDirection;
    private ShiftTimesFilter _shiftFilter;
    private ShiftTimesTarget _shiftTarget;

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
}

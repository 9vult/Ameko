using Ameko.DataModels;
using AssCS;
using Holo;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Ameko.ViewModels
{
    public class ShiftTimesWindowViewModel : ViewModelBase
    {
        public Time ShiftTime { get; set; }

        public ShiftTimesType ShiftTimesType { get; set; }
        public ShiftTimesDirection ShiftTimesDirection { get; set; }
        public ShiftTimesFilter ShiftTimesFilter { get; set; }
        public ShiftTimesTarget ShiftTimesTarget { get; set; }

        public ReactiveCommand<Unit, Unit> ShiftTimesCommand { get; }

        private void DoShiftTimes()
        {
            List<Event> events;
            if (ShiftTimesFilter == ShiftTimesFilter.ALL_ROWS) events = HoloContext.Instance.Workspace.WorkingFile.File.EventManager.Ordered;
            else events = HoloContext.Instance.Workspace.WorkingFile.SelectedEventCollection ?? new List<Event>();

            var sps = events.Select(e => new SnapPosition<Event>(e.Clone(), HoloContext.Instance.Workspace.WorkingFile.File.EventManager.GetBefore(e.Id)?.Id)).ToList();
            HoloContext.Instance.Workspace.WorkingFile.File.HistoryManager.Commit(new Commit<Event>(new Snapshot<Event>(sps, AssCS.Action.EDIT)));

            switch (ShiftTimesType)
            {
                case ShiftTimesType.TIME:
                    foreach (var evt in events)
                    {
                        if (ShiftTimesTarget == ShiftTimesTarget.START || ShiftTimesTarget == ShiftTimesTarget.BOTH)
                        {
                            if (ShiftTimesDirection == ShiftTimesDirection.FORWARD)
                                evt.Start += ShiftTime;
                            else
                                evt.Start -= ShiftTime;
                        }
                        if (ShiftTimesTarget == ShiftTimesTarget.END || ShiftTimesTarget == ShiftTimesTarget.BOTH)
                        {
                            if (ShiftTimesDirection == ShiftTimesDirection.FORWARD)
                                evt.End += ShiftTime;
                            else
                                evt.End -= ShiftTime;
                        }
                    }
                    break;

                // TODO: Frames
                default:
                    return;
            }
        }

        public ShiftTimesWindowViewModel()
        {
            ShiftTime = new Time();
            ShiftTimesType = ShiftTimesType.TIME;
            ShiftTimesDirection = ShiftTimesDirection.FORWARD;
            ShiftTimesFilter = ShiftTimesFilter.SELECTED_ROWS;
            ShiftTimesTarget = ShiftTimesTarget.BOTH;

            ShiftTimesCommand = ReactiveCommand.Create(DoShiftTimes);
        }
    }
}

using AssCS;
using Holo;
using ReactiveUI;
using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows.Input;

namespace Ameko.ViewModels
{
    public class JumpWindowViewModel : ViewModelBase
    {
        public int Frame { get; set; } = 0;
        public Time Time { get; set; } = Time.FromMillis(0);

        public ICommand JumpCommand { get; }
        public Interaction<Unit, Unit> CloseWindow;

        public JumpWindowViewModel() 
        {
            CloseWindow = new Interaction<Unit, Unit>();

            JumpCommand = ReactiveCommand.Create(() =>
            {
                if (Frame != 0)
                {
                    HoloContext.Instance.Workspace.WorkingFile.AVManager.PlaybackController.SeekTo(Frame);
                    CloseWindow.Handle(Unit.Default).Subscribe();
                    return;
                }
                else
                {
                    HoloContext.Instance.Workspace.WorkingFile.AVManager.PlaybackController.SeekTo(Time);
                    CloseWindow.Handle(Unit.Default).Subscribe();
                    return;
                }
            });
        }
    }
}

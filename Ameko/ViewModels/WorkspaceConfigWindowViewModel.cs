using Holo;
using ReactiveUI;
using System.Windows.Input;

namespace Ameko.ViewModels
{
    public class WorkspaceConfigWindowViewModel : ViewModelBase
    {
        private int cps;
        private bool? useSoftLinebreaks;
        public int Cps
        {
            get => cps;
            set => this.RaiseAndSetIfChanged(ref cps, value);
        }

        public bool? UseSoftLinebreaks
        {
            get => useSoftLinebreaks;
            set => this.RaiseAndSetIfChanged(ref useSoftLinebreaks, value);
        }

        public ICommand SaveConfigCommand { get; }

        public WorkspaceConfigWindowViewModel()
        {
            Cps = HoloContext.Instance.Workspace.Cps;
            UseSoftLinebreaks = HoloContext.Instance.Workspace.UseSoftLinebreaks;

            SaveConfigCommand = ReactiveCommand.Create(() =>
            {
                HoloContext.Instance.Workspace.Cps = Cps;
                HoloContext.Instance.Workspace.UseSoftLinebreaks = UseSoftLinebreaks;
            });
        }
    }
}

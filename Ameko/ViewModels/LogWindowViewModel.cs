using Ameko.DataModels;
using Ameko.Services;
using Avalonia.Threading;
using DynamicData;
using Holo;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Ameko.ViewModels
{
    public class LogWindowViewModel : ViewModelBase
    {
        private List<Log> _logs;
        public Interaction<LogWindowViewModel, string?> CopySelectedLogs { get; }
        public ICommand CopySelectedLogsCommand { get; }

        public List<Log> Logs
        {
            get => _logs;
            private set => this.RaiseAndSetIfChanged(ref _logs, value);
        }

        public LogWindowViewModel()
        {
            _logs = new List<Log>(HoloContext.Logger.Logs);
            _logs.Reverse();

            CopySelectedLogs = new Interaction<LogWindowViewModel, string?>();
            CopySelectedLogsCommand = ReactiveCommand.Create(async () => {
                await CopySelectedLogs.Handle(this);
            });

            HoloContext.Logger.PropertyChanged += Logger_LogAdded;
        }

        private void Logger_LogAdded(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Logs = new List<Log>(HoloContext.Logger.Logs);
            Logs.Reverse();
        }
    }
}

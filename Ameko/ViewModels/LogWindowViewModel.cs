using Ameko.DataModels;
using Ameko.Services;
using AssCS;
using Avalonia.Threading;
using DynamicData;
using Holo;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Ameko.ViewModels
{
    public class LogWindowViewModel : ViewModelBase
    {
        private ObservableCollection<Log> _logs;
        public Interaction<LogWindowViewModel, string?> CopySelectedLogs { get; }
        public ICommand CopySelectedLogsCommand { get; }

        public ObservableCollection<Log> Logs
        {
            get => _logs;
            private set => this.RaiseAndSetIfChanged(ref _logs, value);
        }

        public LogWindowViewModel()
        {
            _logs = new ObservableCollection<Log>(HoloContext.Logger.Logs.Reverse());

            CopySelectedLogs = new Interaction<LogWindowViewModel, string?>();
            CopySelectedLogsCommand = ReactiveCommand.Create(async () => {
                await CopySelectedLogs.Handle(this);
            });

            HoloContext.Logger.Logs.CollectionChanged += Logger_LogAdded;
        }

        private void Logger_LogAdded(object? sender, NotifyCollectionChangedEventArgs e)
        {
            e.NewItems?.Cast<Log>().ToList().ForEach(log => _logs.Insert(0, log));
        }
    }
}

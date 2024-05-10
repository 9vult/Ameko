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
    public class ScriptLogWindowViewModel : ViewModelBase
    {
        private string _scriptName;
        private ScriptLogger _logger;
        private string _logs;
        private bool _closeButtonEnabled;
        private int _progress;

        public string ScriptName => _scriptName;

        public string Logs
        {
            get => _logs;
            set => this.RaiseAndSetIfChanged(ref _logs, value);
        }

        public bool CloseButtonEnabled
        {
            get => _closeButtonEnabled;
            set => this.RaiseAndSetIfChanged(ref _closeButtonEnabled, value);
        }

        public int Progress
        {
            get => _progress;
            set => this.RaiseAndSetIfChanged(ref _progress, value);
        }

        public ScriptLogWindowViewModel(string script, ScriptLogger logger, bool closeButtonEnabled, int progress)
        {
            _scriptName = script;
            _logger = logger;
            _closeButtonEnabled = closeButtonEnabled;
            _progress = progress;
            _logs = _logger.Dump();
            _logger.Logs.CollectionChanged += Logs_CollectionChanged;
        }

        private void Logs_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            Logs = _logger.Dump();
        }
    }
}

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
        private string _logs = string.Empty;

        public string ScriptName => _scriptName;

        public string LogString
        {
            get => _logs;
            set => this.RaiseAndSetIfChanged(ref _logs, value);
        }

        public ScriptLogWindowViewModel(string script, ScriptLogger logger)
        {
            _scriptName = script;
            _logger = logger;
            _logs = _logger.Dump();
            _logger.Logs.CollectionChanged += Logs_CollectionChanged;
        }

        private void Logs_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            LogString = _logger.Dump();
        }
    }
}

// SPDX-License-Identifier: GPL-3.0-only

using System.Collections.ObjectModel;
using Holo.Models;
using Holo.Providers;
using ReactiveUI;

namespace Ameko.ViewModels.Windows;

public partial class LogWindowViewModel : ViewModelBase
{
    private readonly ILogProvider _logProvider;

    public object? SelectedLog
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public ReadOnlyObservableCollection<LogEntry> LogEntries => _logProvider.LogEntries;

    public LogWindowViewModel(ILogProvider logProvider)
    {
        _logProvider = logProvider;
        _logProvider.LogEntries.CollectionChanged += (_, args) =>
        {
            if (args.NewItems is not null && args.NewItems.Count > 0)
                SelectedLog = args.NewItems[^1];
        };
    }
}

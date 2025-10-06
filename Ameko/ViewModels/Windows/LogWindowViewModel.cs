// SPDX-License-Identifier: GPL-3.0-only

using System.Collections.ObjectModel;
using Holo.Providers;
using ReactiveUI;

namespace Ameko.ViewModels.Windows;

public partial class LogWindowViewModel : ViewModelBase
{
    private readonly ILogProvider _logProvider;
    private object? _selectedLog;

    public object? SelectedLog
    {
        get => _selectedLog;
        set => this.RaiseAndSetIfChanged(ref _selectedLog, value);
    }

    public ReadOnlyObservableCollection<string> LogEntries => _logProvider.LogEntries;

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

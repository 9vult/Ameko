// SPDX-License-Identifier: GPL-3.0-only

using System.Collections.ObjectModel;
using Holo;
using ReactiveUI;

namespace Ameko.ViewModels.Windows;

public partial class LogWindowViewModel : ViewModelBase
{
    private object? _selectedLog;

    public object? SelectedLog
    {
        get => _selectedLog;
        set => this.RaiseAndSetIfChanged(ref _selectedLog, value);
    }

    public static ReadOnlyObservableCollection<string> LogEntries =>
        HoloContext.Instance.LogEntries;

    public LogWindowViewModel()
    {
        HoloContext.Instance.LogEntries.CollectionChanged += (_, args) =>
        {
            if (args.NewItems is not null && args.NewItems.Count > 0)
                SelectedLog = args.NewItems[^1];
        };
    }
}

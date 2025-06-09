// SPDX-License-Identifier: MPL-2.0

using System.Collections.ObjectModel;
using Holo.IO;

namespace Holo.Providers;

public class LogProvider : ILogProvider
{
    private readonly ObservableCollection<string> _entries;

    public AssCS.Utilities.ReadOnlyObservableCollection<string> LogEntries { get; }

    public LogProvider()
    {
        _entries = [];
        LogEntries = new AssCS.Utilities.ReadOnlyObservableCollection<string>(_entries);
        LoggerHelper.Initialize(_entries);
    }
}

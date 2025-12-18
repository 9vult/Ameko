// SPDX-License-Identifier: MPL-2.0

using Holo.Models;

namespace Holo.Providers;

using AssCS.Utilities;

public interface ILogProvider
{
    /// <summary>
    /// Observable collection of formatted log entries
    /// </summary>
    public ReadOnlyObservableCollection<LogEntry> LogEntries { get; }
}

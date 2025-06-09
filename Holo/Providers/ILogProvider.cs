// SPDX-License-Identifier: MPL-2.0

namespace Holo.Providers;

using AssCS.Utilities;

public interface ILogProvider
{
    /// <summary>
    /// Observable collection of formatted log entries
    /// </summary>
    public ReadOnlyObservableCollection<string> LogEntries { get; }
}

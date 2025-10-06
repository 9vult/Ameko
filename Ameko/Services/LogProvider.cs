// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using Holo.IO;
using Holo.Providers;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace Ameko.Services;

/// <summary>
/// Manages the logger
/// </summary>
internal class LogProvider : ILogProvider
{
    // ${logger}.${callsite:className=false:methodName=true}
    // achieves the same thing as ${callsite}, but allows for custom-named loggers when appropriate.
    // GetCurrentClassLogger() will work as normal, while Scripts inject their QualifiedName for the same effect.
    private const string Layout =
        "${longdate} | ${level:uppercase=true:padding=-5} | ${logger}.${callsite:className=false:methodName=true} → ${message}";

    /// <inheritdoc />
    public AssCS.Utilities.ReadOnlyObservableCollection<string> LogEntries { get; }

    public LogProvider()
    {
        ObservableCollection<string> entries = [];
        LogEntries = new AssCS.Utilities.ReadOnlyObservableCollection<string>(entries);

        var config = new LoggingConfiguration();
        var consoleTarget = new ColoredConsoleTarget("console") { Layout = Layout };
        var fileTarget = new FileTarget("file")
        {
            Layout = Layout,
            FileName = Path.Combine(
                Directories.StateHome,
                "logs",
                $"{DateTime.Now:yyyy-MM-dd}.log"
            ),
        };
        var collectionTarget = new ObservableCollectionTarget(entries) { Layout = Layout };

        config.AddTarget("console", consoleTarget);
        config.AddTarget("file", fileTarget);
        config.AddTarget("collection", collectionTarget);

        config.AddRuleForAllLevels(consoleTarget);
        config.AddRuleForAllLevels(fileTarget);
        config.AddRuleForAllLevels(collectionTarget);

        LogManager.Configuration = config;
    }

    private sealed class ObservableCollectionTarget(
        ObservableCollection<string> entries,
        int maxEntries = 500
    ) : TargetWithLayout
    {
        private readonly SynchronizationContext? _syncContext = SynchronizationContext.Current;

        protected override void Write(LogEventInfo logEvent)
        {
            var message = Layout.Render(logEvent);

            if (entries.Count >= maxEntries)
                entries.RemoveAt(0); // Remove oldest

            if (_syncContext is not null)
            {
                _syncContext.Post(_ => entries.Add(message), null);
            }
            else
            {
                entries.Add(message); // Fallback
            }
        }
    }
}

// SPDX-License-Identifier: MPL-2.0

using System.Collections.ObjectModel;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace Holo.IO;

/// <summary>
/// Manages the logger
/// </summary>
internal static class LoggerHelper
{
    /// <summary>
    /// Set up the logger
    /// </summary>
    /// <param name="logEntries">Observable collection for log viewing</param>
    internal static void Initialize(ObservableCollection<string> logEntries)
    {
        // ${logger}.${callsite:className=false:methodName=true}
        // achieves the same thing as ${callsite}, but allows for custom-named loggers when appropriate.
        // GetCurrentClassLogger() will work as normal, while Scripts inject their QualifiedName for the same effect.
        const string layout =
            "${longdate} | ${level:uppercase=true:padding=-5} | ${logger}.${callsite:className=false:methodName=true} → ${message}";

        var config = new LoggingConfiguration();
        var consoleTarget = new ColoredConsoleTarget("console") { Layout = layout };
        var fileTarget = new FileTarget("file")
        {
            Layout = layout,
            FileName = Path.Combine(
                DirectoryService.StateHome,
                "logs",
                $"{DateTime.Now:yyyy-MM-dd}.log"
            ),
        };
        var collectionTarget = new ObservableCollectionTarget(logEntries) { Layout = layout };

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

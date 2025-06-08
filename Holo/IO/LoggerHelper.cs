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
                Directories.StateHome,
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

    private class ObservableCollectionTarget(ObservableCollection<string> entries)
        : TargetWithLayout
    {
        protected override void Write(LogEventInfo logEvent)
        {
            var message = Layout.Render(logEvent);
            entries.Add(message);
        }
    }
}

// SPDX-License-Identifier: MPL-2.0

using System.Collections.ObjectModel;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace Holo.IO;

/// <summary>
/// Manages the logger
/// </summary>
internal class LoggerHelper
{
    /// <summary>
    /// Set up the logger
    /// </summary>
    /// <param name="logEntries">Observable collection for log viewing</param>
    internal static void Initialize(ObservableCollection<string> logEntries)
    {
        const string layout =
            "${longdate} | ${level:uppercase=true:padding=-5} | ${callsite} → ${message}";

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

        config.AddTarget(consoleTarget);
        config.AddTarget(fileTarget);
        config.AddTarget(collectionTarget);

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
            string message = this.Layout.Render(logEvent);
            entries.Add(message);
        }
    }
}

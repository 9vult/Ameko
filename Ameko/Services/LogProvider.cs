// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using Holo.IO;
using Holo.Models;
using Holo.Providers;
using NLog;
using NLog.Config;
using NLog.Layouts;
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
    private const string PrintLayout =
        "${longdate} | ${level:uppercase=true:padding=-5} | ${logger}.${callsite:className=false:methodName=true} → ${message}";

    private static readonly JsonLayout FileLayout = new()
    {
        RenderEmptyObject = false,
        Attributes =
        {
            new JsonAttribute("timestamp", "${longdate}"),
            new JsonAttribute("level", "${level:uppercase=true}"),
            new JsonAttribute("origin", "${logger}.${callsite:className=false:methodName=true}"),
            new JsonAttribute("message", "${message}"),
            new JsonAttribute("exception", "${exception:format=toString}"),
            new JsonAttribute(
                "properties",
                new JsonLayout { IncludeEventProperties = true },
                encode: false
            ),
        },
    };

    /// <inheritdoc />
    public AssCS.Utilities.ReadOnlyObservableCollection<LogEntry> LogEntries { get; }

    public LogProvider()
    {
        ObservableCollection<LogEntry> entries = [];
        LogEntries = new AssCS.Utilities.ReadOnlyObservableCollection<LogEntry>(entries);

        var config = new LoggingConfiguration();
        var consoleTarget = new ColoredConsoleTarget("console") { Layout = PrintLayout };
        var collectionTarget = new ObservableCollectionTarget(entries) { Layout = PrintLayout };
        var fileTarget = new FileTarget("file")
        {
            Layout = FileLayout,
            FileName = Path.Combine(
                Directories.StateHome,
                "logs",
                $"{DateTime.Now:yyyy-MM-dd}.json"
            ),
        };

        config.AddTarget("console", consoleTarget);
        config.AddTarget("collection", collectionTarget);
        config.AddTarget("file", fileTarget);

        config.AddRuleForAllLevels(consoleTarget);
        config.AddRuleForAllLevels(collectionTarget);
        config.AddRuleForAllLevels(fileTarget);

        LogManager.Configuration = config;
    }

    private sealed class ObservableCollectionTarget(
        ObservableCollection<LogEntry> entries,
        int maxEntries = 500
    ) : TargetWithLayout
    {
        private readonly SynchronizationContext? _syncContext = SynchronizationContext.Current;

        protected override void Write(LogEventInfo logEvent)
        {
            var message = Layout.Render(logEvent);
            var logLevel = ConvertLogLevel(logEvent.Level);

            if (entries.Count >= maxEntries)
                entries.RemoveAt(0); // Remove oldest

            if (_syncContext is not null)
            {
                _syncContext.Post(_ => entries.Add(new LogEntry(logLevel, message)), null);
            }
            else
            {
                entries.Add(new LogEntry(logLevel, message)); // Fallback
            }
        }

        private static Microsoft.Extensions.Logging.LogLevel ConvertLogLevel(LogLevel input)
        {
            if (input == LogLevel.Info)
                return Microsoft.Extensions.Logging.LogLevel.Information;
            if (input == LogLevel.Debug)
                return Microsoft.Extensions.Logging.LogLevel.Debug;
            if (input == LogLevel.Trace)
                return Microsoft.Extensions.Logging.LogLevel.Trace;
            if (input == LogLevel.Warn)
                return Microsoft.Extensions.Logging.LogLevel.Warning;
            if (input == LogLevel.Error)
                return Microsoft.Extensions.Logging.LogLevel.Error;
            if (input == LogLevel.Fatal)
                return Microsoft.Extensions.Logging.LogLevel.Critical;
            return Microsoft.Extensions.Logging.LogLevel.None;
        }
    }
}

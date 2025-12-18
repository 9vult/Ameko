// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using Holo.IO;
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
    public AssCS.Utilities.ReadOnlyObservableCollection<string> LogEntries { get; }

    public LogProvider()
    {
        ObservableCollection<string> entries = [];
        LogEntries = new AssCS.Utilities.ReadOnlyObservableCollection<string>(entries);

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

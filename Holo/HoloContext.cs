// SPDX-License-Identifier: MPL-2.0

using System.Collections.ObjectModel;
using Holo.IO;
using NLog;

namespace Holo;

/// <summary>
/// Main class for the Holo library
/// </summary>
/// <remarks>
/// <para>
/// Holo is a "middleware" library designed to couple the Ameko frontend
/// with the AssCS backend. As an extension of the Model-View-Controler
/// paradigm, the frontend should contain as little "code-behind" as
/// possible, instead using actions to call into Holo.
/// </para><para>
/// Holo also provides plugin and API support for Ameko and third-parties
/// to use.
/// </para>
/// </remarks>
public class HoloContext
{
    // ReSharper disable once InconsistentNaming
    private static readonly Lazy<HoloContext> _instance = new(() => new HoloContext());
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    /// <summary>
    /// Holo instance
    /// </summary>
    /// <remarks>Not initialized until first request</remarks>
    public static HoloContext Instance => _instance.Value;

    /// <summary>
    /// Observable collection of formatted log entries
    /// </summary>
    public ReadOnlyObservableCollection<string> LogEntries { get; }

    private HoloContext()
    {
        ObservableCollection<string> logEntries = [];
        LogEntries = new ReadOnlyObservableCollection<string>(logEntries);

        Directories.Create();
        LoggerHelper.Initialize(logEntries);

        Logger.Info("Initializing Holo");

        Logger.Info("Initialization complete");
    }
}

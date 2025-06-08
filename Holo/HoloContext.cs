// SPDX-License-Identifier: MPL-2.0

using System.Collections.ObjectModel;
using AssCS;
using Holo.IO;
using Holo.Scripting;
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
public class HoloContext : BindableBase
{
    // ReSharper disable once InconsistentNaming
    private static readonly Lazy<HoloContext> _instance = new(() => new HoloContext());
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    private Solution _solution;

    /// <summary>
    /// Holo instance
    /// </summary>
    /// <remarks>Not initialized until first request</remarks>
    public static HoloContext Instance => _instance.Value;

    /// <summary>
    /// Observable collection of formatted log entries
    /// </summary>
    public AssCS.Utilities.ReadOnlyObservableCollection<string> LogEntries { get; }

    /// <summary>
    /// The currently-loaded solution
    /// </summary>
    public Solution Solution
    {
        get => _solution;
        set => SetProperty(ref _solution, value);
    }

    /// <summary>
    /// Application-level configuration options
    /// </summary>
    public Configuration Configuration { get; }

    /// <summary>
    /// Globally-accessible objects
    /// </summary>
    public Globals Globals { get; }

    public DependencyControl DependencyControl { get; }

    private HoloContext()
    {
        ObservableCollection<string> logEntries = [];
        LogEntries = new AssCS.Utilities.ReadOnlyObservableCollection<string>(logEntries);

        Directories.Create();
        LoggerHelper.Initialize(logEntries);

        Logger.Info("Initializing Holo");

        Configuration = Configuration.Parse(Paths.Configuration);
        Configuration.Save();

        Globals = Globals.Parse(Paths.Globals);
        Globals.Save();

        DependencyControl = new DependencyControl();
        Task.Run(() => DependencyControl.SetUpBaseRepository())
            .ContinueWith(_ => DependencyControl.BootstrapFromList(Configuration.RepositoryUrls));

        _solution = new Solution();

        Logger.Info("Initialization complete");
    }
}

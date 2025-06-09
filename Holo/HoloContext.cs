// SPDX-License-Identifier: MPL-2.0

using System.Collections.ObjectModel;
using AssCS;
using Holo.IO;
using Holo.Providers;
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
public class HoloContext : BindableBase, IHoloContext
{
    // ReSharper disable once InconsistentNaming
    private static readonly Lazy<HoloContext> _instance = new(() => new HoloContext());
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    private Solution _solution;

    /// <summary>
    /// Holo instance
    /// </summary>
    /// <remarks>Not initialized until first request</remarks>
    [Obsolete("Use Dependency Injection instead.")]
    public static HoloContext Instance => _instance.Value;

    /// <summary>
    /// The currently-loaded solution
    /// </summary>
    [Obsolete("Use Dependency Injection instead.")]
    public Solution Solution
    {
        get => _solution;
        set => SetProperty(ref _solution, value);
    }

    /// <summary>
    /// Provides the currently-loaded <see cref="Solution"/>
    /// </summary>
    public ISolutionProvider SolutionProvider { get; }

    /// <summary>
    /// Application-level configuration options
    /// </summary>
    public Configuration Configuration { get; }

    /// <summary>
    /// Globally-accessible objects
    /// </summary>
    public Globals Globals { get; }

    public DependencyControl DependencyControl { get; }

    public HoloContext()
    {
        Directories.Create();

        Logger.Info("Initializing Holo");

        Configuration = Configuration.Parse(Paths.Configuration);
        Configuration.Save();

        Globals = Globals.Parse(Paths.Globals);
        Globals.Save();

        DependencyControl = new DependencyControl();
        Task.Run(() => DependencyControl.SetUpBaseRepository())
            .ContinueWith(_ =>
                DependencyControl.AddAdditionalRepositories(Configuration.RepositoryUrls)
            );

        _solution = new Solution();
        SolutionProvider = new SolutionProvider();

        Logger.Info("Initialization complete");
    }
}

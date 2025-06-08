// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CSScriptLib;
using Holo.IO;
using Holo.Scripting;
using NLog;

namespace Ameko.Services;

/// <summary>
/// Service for executing <see cref="HoloScript"/>s
/// </summary>
public class ScriptService
{
    // ReSharper disable once InconsistentNaming
    private static readonly Lazy<ScriptService> _instance = new(() => new ScriptService());
    private static readonly Uri ScriptsRoot = new(Path.Combine(Directories.DataHome, "scripts"));
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    private readonly ObservableCollection<HoloScript> _scripts;
    private readonly Dictionary<string, HoloScript?> _scriptMap;

    public static ScriptService Instance => _instance.Value;

    public AssCS.Utilities.ReadOnlyObservableCollection<HoloScript> Scripts { get; }

    /// <summary>
    /// Get a script by its qualified name
    /// </summary>
    /// <param name="qualifiedName">Name of the script</param>
    /// <param name="script">Script, if found</param>
    /// <returns><see langword="true"/> if found</returns>
    public bool TryGetScript(string qualifiedName, [NotNullWhen(true)] out HoloScript? script)
    {
        return _scriptMap.TryGetValue(qualifiedName, out script);
    }

    /// <summary>
    /// Asynchronously execute a script or method
    /// </summary>
    /// <param name="qualifiedName">Name of the script or method</param>
    /// <returns>Execution result of the script or method</returns>
    /// <remarks>
    /// This method attempts to execute a script first, then a method.
    /// This relies on method names being <c>scriptName.methodName</c>.
    /// </remarks>
    public async Task<ExecutionResult> ExecuteScriptAsync(string qualifiedName)
    {
        // Try running as a script
        if (TryGetScript(qualifiedName, out var script))
        {
            return await script.ExecuteAsync();
        }

        // Try running as an exported function
        var scriptName = qualifiedName[..qualifiedName.LastIndexOf('.')];
        var methodName = qualifiedName[(qualifiedName.LastIndexOf('.') + 1)..];
        if (TryGetScript(scriptName, out script))
        {
            return await script.ExecuteAsync(methodName);
        }

        // Not found
        Logger.Error($"Script or method not found: {qualifiedName}");
        return new ExecutionResult
        {
            Status = ExecutionStatus.Failure,
            Message = $"Script or method not found: {qualifiedName}",
        };
    }

    public void Reload(bool isManual)
    {
        Logger.Info("Reloading scripts");
        if (!Directory.Exists(ScriptsRoot.LocalPath))
        {
            Logger.Error("Scripts directory not found");
            return;
        }

        _scripts.Clear();

        foreach (
            var path in Directory
                .EnumerateFiles(ScriptsRoot.LocalPath, "*.cs")
                .Where(f => !f.EndsWith(".lib.cs")) // Exclude libraries from being loaded
        )
        {
            try
            {
                Logger.Trace($"Loading script {path}");
                var script = CSScript.Evaluator.LoadFile<HoloScript>(path);
                if (script is null)
                {
                    Logger.Warn($"Script {path} was invalid");
                    continue;
                }

                _scripts.Add(script);
                _scriptMap.Add(script.Info.QualifiedName, script);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                continue;
            }

            // For informational purposes
            var libCount = Directory.GetFiles(ScriptsRoot.LocalPath, "*.lib.cs").Length;

            Logger.Info($"Reloaded {_scripts.Count} scripts ({libCount} libraries)");
        }
    }

    private ScriptService()
    {
        _scripts = [];
        _scriptMap = [];
        Scripts = new AssCS.Utilities.ReadOnlyObservableCollection<HoloScript>(_scripts);
        Reload(false);
    }
}

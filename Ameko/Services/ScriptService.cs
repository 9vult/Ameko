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
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using NLog;

namespace Ameko.Services;

/// <summary>
/// Service for executing <see cref="HoloScript"/>s
/// </summary>
public class ScriptService
{
    private static readonly Uri ScriptsRoot = new(Path.Combine(Directories.DataHome, "scripts"));
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    private readonly ObservableCollection<HoloScript> _scripts;
    private readonly Dictionary<string, HoloScript?> _scriptMap;

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

    /// <summary>
    /// Reload scripts
    /// </summary>
    /// <param name="isManual">Whether to show the message box</param>
    public void Reload(bool isManual)
    {
        Logger.Info("Reloading scripts");
        if (!Directory.Exists(ScriptsRoot.LocalPath))
        {
            Logger.Error("Scripts directory not found");
            return;
        }

        _scripts.Clear();
        _scriptMap.Clear();

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
        }
        // For informational purposes
        var libCount = Directory.GetFiles(ScriptsRoot.LocalPath, "*.lib.cs").Length;

        Logger.Info($"Reloaded {_scripts.Count} scripts ({libCount} libraries)");

        // Execute event
        OnReload?.Invoke(this, EventArgs.Empty);

        // Display message box (if manually invoked)
        if (isManual)
            _ = DisplayMessageBoxAsync(I18N.Resources.MsgBox_ScriptService_Reload_Body);
    }

    private static async Task DisplayMessageBoxAsync(string message)
    {
        var box = MessageBoxManager.GetMessageBoxStandard(
            I18N.Resources.MsgBox_ScriptService_Title,
            message
        );
        await box.ShowAsync();
    }

    public ScriptService()
    {
        _scripts = [];
        _scriptMap = [];
        Scripts = new AssCS.Utilities.ReadOnlyObservableCollection<HoloScript>(_scripts);
        Reload(false);
    }

    /// <summary>
    /// Event Handler for <see cref="ScriptService.Reload"/>
    /// </summary>
    public delegate void ReloadEventHandler(object sender, EventArgs e);
    public event ReloadEventHandler? OnReload;
}

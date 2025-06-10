// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Threading;
using CSScriptLib;
using Holo.IO;
using Holo.Scripting;
using MsBox.Avalonia;
using NLog;

namespace Ameko.Services;

/// <summary>
/// Service for executing <see cref="HoloScript"/>s
/// </summary>
public class ScriptService : IScriptService
{
    private static readonly Uri ScriptsRoot = new(
        Path.Combine(DirectoryService.DataHome, "scripts")
    );
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    private readonly ObservableCollection<HoloScript> _scripts;
    private readonly Dictionary<string, HoloScript?> _scriptMap;

    /// <inheritdoc cref="IScriptService.Scripts"/>
    public AssCS.Utilities.ReadOnlyObservableCollection<HoloScript> Scripts { get; }

    /// <inheritdoc cref="IScriptService.TryGetScript"/>
    public bool TryGetScript(string qualifiedName, [NotNullWhen(true)] out HoloScript? script)
    {
        return _scriptMap.TryGetValue(qualifiedName, out script);
    }

    /// <inheritdoc cref="IScriptService.ExecuteScriptAsync"/>
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

    /// <inheritdoc cref="IScriptService.Reload"/>
    public async Task Reload(bool isManual)
    {
        Logger.Info("Reloading scripts");
        if (!Directory.Exists(ScriptsRoot.LocalPath))
            Directory.CreateDirectory(ScriptsRoot.LocalPath);

        var scriptPaths = Directory
            .EnumerateFiles(ScriptsRoot.LocalPath, "*.cs")
            .Where(f => !f.EndsWith(".lib.cs"));

        List<HoloScript> loadedScripts = [];

        foreach (var path in scriptPaths)
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

                loadedScripts.Add(script);
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

        // Update UI-bound collections and fire event on the UI thread for safety
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            _scripts.Clear();
            _scriptMap.Clear();

            foreach (var script in loadedScripts)
            {
                _scripts.Add(script);
                _scriptMap.Add(script.Info.QualifiedName, script);
            }

            // Fire event
            OnReload?.Invoke(this, EventArgs.Empty);
        });

        // Display message box (if manually invoked)
        if (isManual)
            await DisplayMessageBoxAsync(I18N.Resources.MsgBox_ScriptService_Reload_Body);
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
    }

    public event IScriptService.ReloadEventHandler? OnReload;
}

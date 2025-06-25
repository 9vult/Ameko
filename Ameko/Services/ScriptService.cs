// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using Ameko.Converters;
using AssCS.History;
using Avalonia.Threading;
using CSScriptLib;
using Holo.IO;
using Holo.Providers;
using Holo.Scripting;
using Holo.Scripting.Models;
using Jint;
using Jint.Native;
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

    private readonly IFileSystem _fileSystem;
    private readonly ISolutionProvider _solutionProvider;
    private readonly ObservableCollection<IHoloExecutable> _scripts;
    private readonly Dictionary<string, HoloScript?> _scriptMap;
    private readonly Dictionary<string, HoloScriptlet?> _scriptletMap;

    /// <inheritdoc />
    public AssCS.Utilities.ReadOnlyObservableCollection<IHoloExecutable> Scripts { get; }

    /// <inheritdoc />
    public bool TryGetScript(string qualifiedName, [NotNullWhen(true)] out HoloScript? script)
    {
        return _scriptMap.TryGetValue(qualifiedName, out script);
    }

    /// <inheritdoc />
    public bool TryGetScriptlet(string qualifiedName, [NotNullWhen(true)] out HoloScriptlet? script)
    {
        return _scriptletMap.TryGetValue(qualifiedName, out script);
    }

    /// <inheritdoc />
    public async Task<ExecutionResult> ExecuteScriptAsync(string qualifiedName)
    {
        // Try running as a script
        if (TryGetScript(qualifiedName, out var script))
        {
            return await script.ExecuteAsync();
        }

        // Try running as an exported function

        if (qualifiedName.LastIndexOf('+') >= 0)
        {
            var scriptName = qualifiedName[..qualifiedName.LastIndexOf('+')];
            var methodName = qualifiedName[(qualifiedName.LastIndexOf('+') + 1)..];
            if (TryGetScript(scriptName, out script))
            {
                return await script.ExecuteAsync(methodName);
            }
        }

        // Try running a scriptlet
        if (TryGetScriptlet(qualifiedName, out var scriptlet))
        {
            var engine = new Engine(cfg => cfg.AllowClr())
                .SetValue("CommitType", typeof(CommitType))
                .SetValue("logger", LogManager.GetLogger(scriptlet.Info.QualifiedName));

            var success = engine
                .Execute(scriptlet.CompiledScript)
                .Invoke("execute", _solutionProvider.Current);

            return success is JsBoolean jsBool
                ? jsBool.AsBoolean()
                    ? ExecutionResult.Success
                    : new ExecutionResult { Status = ExecutionStatus.Failure }
                : ExecutionResult.Success;
        }

        // Not found
        Logger.Error($"Script or method not found: {qualifiedName}");
        return new ExecutionResult
        {
            Status = ExecutionStatus.Failure,
            Message = $"Script or method not found: {qualifiedName}",
        };
    }

    /// <inheritdoc />
    public async Task Reload(bool isManual)
    {
        Logger.Info("Reloading scripts...");
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
                Logger.Trace($"Loading script {path}...");
                var script = CSScript.Evaluator.LoadFile<HoloScript>(path);
                if (script is null)
                {
                    Logger.Warn($"Script {path} was invalid!");
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

        List<HoloScriptlet> loadedScriptlets = [];

        var scriptletPaths = Directory.EnumerateFiles(ScriptsRoot.LocalPath, "*.js");

        foreach (var path in scriptletPaths)
        {
            try
            {
                Logger.Trace($"Loading scriptlet {path}...");
                await using var fs = _fileSystem.FileStream.New(
                    path,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.ReadWrite
                );
                using var reader = new StreamReader(fs);
                var compiled = Engine.PrepareScript(await reader.ReadToEndAsync());
                var scriptletInfo = new Engine().Execute(compiled).Evaluate("scriptInfo");
                loadedScriptlets.Add(
                    new HoloScriptlet
                    {
                        Info = new ModuleInfo
                        {
                            DisplayName = scriptletInfo.Get("displayName").ToString(),
                            QualifiedName = scriptletInfo.Get("qualifiedName").ToString(),
                        },
                        CompiledScript = compiled,
                    }
                );
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        // For informational purposes
        var libCount = Directory.GetFiles(ScriptsRoot.LocalPath, "*.lib.cs").Length;
        Logger.Info(
            $"Reloaded {loadedScripts.Count} scripts ({libCount} libraries) and {loadedScriptlets.Count} scriptlets"
        );

        // Update UI-bound collections and fire event on the UI thread for safety
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            _scripts.Clear();
            _scriptMap.Clear();
            _scriptletMap.Clear();

            foreach (var script in loadedScripts)
            {
                _scripts.Add(script);
                _scriptMap.Add(script.Info.QualifiedName, script);
            }

            foreach (var script in loadedScriptlets)
            {
                _scripts.Add(script);
                _scriptletMap.Add(script.Info.QualifiedName, script);
            }

            // Fire event
            OnReload?.Invoke(this, EventArgs.Empty);
        });

        // Display message box (if manually invoked)
        if (isManual)
            await DisplayMessageBoxAsync(I18N.Other.MsgBox_ScriptService_Reload_Body);
    }

    /// <inheritdoc />
    public event EventHandler<EventArgs>? OnReload;

    private static async Task DisplayMessageBoxAsync(string message)
    {
        var box = MessageBoxManager.GetMessageBoxStandard(
            I18N.Other.MsgBox_ScriptService_Title,
            message
        );
        await box.ShowAsync();
    }

    public ScriptService(IFileSystem fileSystem, ISolutionProvider solutionProvider)
    {
        _fileSystem = fileSystem;
        _solutionProvider = solutionProvider;

        _scripts = [];
        _scriptMap = [];
        _scriptletMap = [];
        Scripts = new AssCS.Utilities.ReadOnlyObservableCollection<IHoloExecutable>(_scripts);
    }
}

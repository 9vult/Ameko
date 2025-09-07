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
using Ameko.Views.Sdk;
using AssCS.History;
using Avalonia.Controls;
using Avalonia.Threading;
using CSScriptLib;
using Holo.Configuration.Keybinds;
using Holo.IO;
using Holo.Models;
using Holo.Providers;
using Holo.Scripting;
using Holo.Scripting.Models;
using Jint;
using Jint.Native;
using NLog;

namespace Ameko.Services;

/// <summary>
/// Service for executing <see cref="HoloScript"/>s
/// </summary>
public class ScriptService : IScriptService
{
    private static readonly Uri ScriptsRoot = new(Path.Combine(Directories.DataHome, "scripts"));
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    private readonly IFileSystem _fileSystem;
    private readonly IProjectProvider _projectProvider;
    private readonly IKeybindRegistrar _keybindRegistrar;
    private readonly IMessageBoxService _messageBoxService;
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
            try
            {
                return await script.ExecuteAsync();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error executing script");
                return new ExecutionResult
                {
                    Status = ExecutionStatus.Failure,
                    Message = ex.ToString(),
                };
            }
        }

        // Try running as an exported function

        if (qualifiedName.LastIndexOf('+') >= 0)
        {
            var scriptName = qualifiedName[..qualifiedName.LastIndexOf('+')];
            var methodName = qualifiedName[(qualifiedName.LastIndexOf('+') + 1)..];
            if (TryGetScript(scriptName, out script))
            {
                try
                {
                    return await script.ExecuteAsync(methodName);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Error executing script");
                    return new ExecutionResult
                    {
                        Status = ExecutionStatus.Failure,
                        Message = ex.ToString(),
                    };
                }
            }
        }

        // Try running a scriptlet
        if (TryGetScriptlet(qualifiedName, out var scriptlet))
        {
            var engine = new Engine(cfg => cfg.AllowClr())
                .SetValue("ChangeType", typeof(ChangeType))
                .SetValue("logger", LogManager.GetLogger(scriptlet.Info.QualifiedName));

            var success = engine
                .Execute(scriptlet.CompiledScript)
                .Invoke("execute", _projectProvider.Current);

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
    public string ExecutePlaygroundScript(string content, bool csharp)
    {
        if (!csharp)
            throw new NotImplementedException();

        try
        {
            _ = CSScript.Evaluator.Eval(content);
            return I18N.Playground.Playground_Status_Success;
        }
        catch (Exception ex)
        {
            return string.Format(I18N.Playground.Playground_Status_Failure, ex);
        }
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
                Logger.Debug($"Loading script {path}...");
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
                Logger.Debug($"Loading scriptlet {path}...");
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

            var qnames = new HashSet<string>();

            foreach (var script in loadedScripts)
            {
                _scripts.Add(script);
                _scriptMap.Add(script.Info.QualifiedName, script);

                if (!script.Info.Headless)
                    qnames.Add(script.Info.QualifiedName);
                foreach (var export in script.Info.Exports)
                    qnames.Add($"{script.Info.QualifiedName}+{export.QualifiedName}");
            }

            foreach (var script in loadedScriptlets)
            {
                _scripts.Add(script);
                _scriptletMap.Add(script.Info.QualifiedName, script);

                qnames.Add(script.Info.QualifiedName);
            }

            // Register the script qualified names for keybinding
            _keybindRegistrar.RegisterKeybinds(
                qnames.Select(name => new Keybind(name, null, KeybindContext.None)).ToList(),
                true
            );

            // Fire event
            OnReload?.Invoke(this, EventArgs.Empty);
        });

        // Display message box (if manually invoked)
        if (isManual)
        {
            await _messageBoxService.ShowAsync(
                I18N.Other.MsgBox_ScriptService_Title,
                I18N.Other.MsgBox_ScriptService_Reload_Body,
                MessageBoxButtons.Ok
            );
        }
    }

    /// <inheritdoc />
    public event EventHandler<EventArgs>? OnReload;

    public ScriptService(
        IFileSystem fileSystem,
        IProjectProvider projectProvider,
        IKeybindRegistrar keybindRegistrar,
        IMessageBoxService messageBoxService
    )
    {
        _fileSystem = fileSystem;
        _projectProvider = projectProvider;
        _keybindRegistrar = keybindRegistrar;
        _messageBoxService = messageBoxService;

        _scripts = [];
        _scriptMap = [];
        _scriptletMap = [];
        Scripts = new AssCS.Utilities.ReadOnlyObservableCollection<IHoloExecutable>(_scripts);
    }
}

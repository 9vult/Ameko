// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using CSScriptLib;
using Holo.IO;
using Holo.Scripting;
using NLog;

namespace Ameko.Services;

public class ScriptService
{
    // ReSharper disable once InconsistentNaming
    private static readonly Lazy<ScriptService> _instance = new(() => new ScriptService());
    private static readonly Uri ScriptsRoot = new(Path.Combine(Directories.DataHome, "scripts"));
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    private readonly ObservableCollection<HoloScript> _scripts;

    public static ScriptService Instance => _instance.Value;

    public AssCS.Utilities.ReadOnlyObservableCollection<HoloScript> Scripts { get; }

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
        Scripts = new AssCS.Utilities.ReadOnlyObservableCollection<HoloScript>(_scripts);
        Reload(false);
    }
}

﻿using Ameko.ViewModels;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using CSScriptLib;
using Holo;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Ameko.Services
{
    public class ScriptService
    {
        private static readonly Lazy<ScriptService> _instance = new(() => new ScriptService());
        private readonly string scriptRoot;
        private readonly Dictionary<string, HoloScript> scripts;
        private readonly Dictionary<string, string[]> functions;
        
        private MainViewModel? mainViewModel;
        public MainViewModel MainViewModel { set { mainViewModel = value; } }
        
        public static ScriptService Instance => _instance.Value;
        public ObservableCollection<Tuple<string, string>> LoadedScripts { get; private set; }

        public List<HoloScript> HoloScripts => new List<HoloScript>(scripts.Values);
        public Dictionary<string, string[]> FunctionMap => new Dictionary<string, string[]>(functions);

        /// <summary>
        /// Get a script by its qualified name
        /// </summary>
        /// <param name="qualifiedName"></param>
        /// <returns></returns>
        public HoloScript? Get(string qualifiedName)
        {
            if (scripts.TryGetValue(qualifiedName, out HoloScript? value))
                return value;
            return null;
        }

        /// <summary>
        /// Execute a script or function
        /// </summary>
        /// <remarks>
        /// Execution as a script will be attempted first, followed by as a function
        /// </remarks>
        /// <param name="qname">Qualified name of the script or function</param>
        /// <returns>Execution result of the script or function</returns>
        public async Task<ExecutionResult> Execute(string qname)
        {
            // Try running as a script
            if (scripts.TryGetValue(qname, out HoloScript? script))
            {
                script.Logger.Reset();
                LogDisplayPreExecution(script);
                var result = await script.Execute();
                LogDisplayPostExecution(script);                
                return result;
            }

            // Try running as a method
            var scriptName = qname[..qname.LastIndexOf('.')];
            if (scripts.TryGetValue(scriptName, out HoloScript? methodScript))
            {
                methodScript.Logger.Reset();
                LogDisplayPreExecution(methodScript);
                var result = await methodScript.Execute(qname);
                LogDisplayPostExecution(methodScript);
                return result;
            }

            // Neither of these worked, fail
            HoloContext.Logger.Error($"The script or function {qname} could not be found.", "ScriptService");
            return new ExecutionResult { Status = ExecutionStatus.Failure, Message = $"The script or function {qname} could not be found." };
        }

        /// <summary>
        /// Execute a freeform script
        /// </summary>
        /// <param name="data">Script data</param>
        /// <returns>Success or the error message</returns>
        public string ExecuteFreeform(string data)
        {
            try
            {
                _ = CSScript.Evaluator.Eval(data);
                return "Success";
            }
            catch (Exception ex)
            {
                HoloContext.Logger.Error(ex.Message, "Playground");
                DisplayMessageBox(ex.ToString());
                return "Error";
            }
        }

        /// <summary>
        /// Reload the scripts
        /// </summary>
        /// <param name="manual">Was the reload manually triggered</param>
        public void Reload(bool manual)
        {
            HoloContext.Logger.Info($"Reloading scripts", "ScriptService");
            if (!Directory.Exists(scriptRoot))
            {
                Directory.CreateDirectory(scriptRoot);
            }

            scripts.Clear();
            functions.Clear();
            LoadedScripts.Clear();

            foreach (var path in Directory.EnumerateFiles(scriptRoot))
            {
                try
                {
                    if (!System.IO.Path.GetExtension(path).Equals(".cs")) continue;
                    HoloContext.Logger.Info($"Loading script {path}", "ScriptService");
                    HoloScript script = CSScript.Evaluator.LoadFile<HoloScript>(path);
                    if (script == null) continue;

                    var name = script.Name;
                    var qname = script.QualifiedName;
                    scripts[qname] = script;
                    LoadedScripts.Add(new Tuple<string, string>(qname, name));

                    if (script.ExportedMethods != null)
                        functions[qname] = script.ExportedMethods;
                }
                catch (Exception e)
                {
                    HoloContext.Logger.Error(e.Message.Trim(), "ScriptService");
                    continue;
                }
            }
            if (manual)
            {
                DisplayMessageBox("Scripts have been reloaded.");
            }
            HoloContext.Logger.Info($"Reloading scripts complete", "ScriptService");
        }

        private static async void DisplayMessageBox(string message)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Ameko Script Service", message, ButtonEnum.Ok);
            await box.ShowAsync();
        }

        private void LogDisplayPreExecution(HoloScript script)
        {
            if (script.LogDisplay is LogDisplay.Ephemeral or LogDisplay.Forced)
            {
                var dialogvm = new ScriptLogWindowViewModel(script.Name, script.Logger);
                mainViewModel?.ShowScriptLogsDialogCommand.Execute(dialogvm);
            }
        }

        private void LogDisplayPostExecution(HoloScript script)
        {
            if (script.LogDisplay == LogDisplay.OnError && script.Logger.LoggedError)
            {
                var dialogvm = new ScriptLogWindowViewModel(script.Name, script.Logger);
                mainViewModel?.ShowScriptLogsDialogCommand.Execute(dialogvm);
            }
            else if (script.LogDisplay == LogDisplay.Ephemeral)
            {
                mainViewModel?.CloseScriptLogsDialogCommand.Execute(Unit.Default);
            }
        }

        private ScriptService()
        {
            scriptRoot = System.IO.Path.Combine(HoloContext.Directories.HoloDataHome, "scripts");
            LoadedScripts = [];
            scripts = [];
            functions = [];
            Reload(false);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using Tomlet;

namespace Holo
{
    public class ConfigurationManager : INotifyPropertyChanged
    {
        private readonly string _configFilePath;
        private readonly string _keybindsFilePath;

        private string _audioProvider;
        private string _videoProvider;
        private int _cps;
        private bool _autosave;
        private int _autosaveInterval;
        private bool _useSoftLinebreaks;
        private ObservableCollection<string> _repositories;
        private Dictionary<string, string> _submenuOverrides;
        private Dictionary<string, List<string>> _installedScripts;
        private KeybindsRegistry _keybindsRegistry;

        /// <summary>
        /// Character-per-second warning limit
        /// </summary>
        public int Cps
        {
            get => _cps;
            set { _cps = value; OnPropertyChanged(nameof(Cps)); WriteConfig(); }
        }

        /// <summary>
        /// Is autosave enabled
        /// </summary>
        public bool Autosave
        {
            get => _autosave;
            set { _autosave = value; OnPropertyChanged(nameof(Autosave)); WriteConfig(); }
        }

        /// <summary>
        /// Autosave writing interval in seconds
        /// </summary>
        public int AutosaveInterval
        {
            get => _autosaveInterval;
            set { _autosaveInterval = value; OnPropertyChanged(nameof(AutosaveInterval)); WriteConfig(); }
        }

        /// <summary>
        /// Choose whether to use soft line breaks (\n) instead
        /// of hard line breaks (\N) default
        /// </summary>
        public bool UseSoftLinebreaks
        {
            get => _useSoftLinebreaks;
            set { _useSoftLinebreaks = value; OnPropertyChanged(nameof(UseSoftLinebreaks)); WriteConfig(); }
        }

        /// <summary>
        /// Add a new DepCtl repository
        /// </summary>
        /// <param name="repoUrl"></param>
        public void AddRepository(string repoUrl)
        {
            _repositories.Add(repoUrl);
            WriteConfig();
        }

        /// <summary>
        /// Add a script to the list of installed scripts
        /// </summary>
        /// <param name="repoUrl">Repository URL</param>
        /// <param name="qname">Qualified name of the script</param>
        public void AddScript(string repoUrl, string qname, bool write = true)
        {
            if (!_installedScripts.ContainsKey(repoUrl))
                _installedScripts[repoUrl] = new List<string>();
            if (!_installedScripts[repoUrl].Contains(qname))
                _installedScripts[repoUrl].Add(qname);
            if (write)
                WriteConfig();
        }

        /// <summary>
        /// Import a list of installed scripts
        /// </summary>
        /// <param name="scripts"></param>
        public void ImportScriptList(Dictionary<string, List<string>> scripts)
        {
            foreach (var pair in scripts)
            {
                foreach (var script in pair.Value)
                {
                    AddScript(pair.Key, script, false);
                }
            }
            WriteConfig();
        }

        /// <summary>
        /// Remove a script from the list of installed scripts
        /// </summary>
        /// <param name="qname">Qualified name of the script</param>
        /// <returns>True if the script was removed</returns>
        public bool RemoveScript(string qname)
        {
            foreach (var pair in _installedScripts)
            {
                if (pair.Value.Contains(qname))
                {
                    var res = pair.Value.Remove(qname);
                    if (pair.Value.Count == 0)
                        _installedScripts.Remove(pair.Key);
                    WriteConfig();
                    return res;
                }
            }
            return false;
        }

        /// <summary>
        /// Remove a DepCtl repository
        /// </summary>
        /// <param name="repoUrl"></param>
        /// <returns>True if it was removed</returns>
        public bool RemoveRepository(string repoUrl)
        {
            var removed = _repositories.Remove(repoUrl);
            if (removed) WriteConfig();
            return removed;
        }

        /// <summary>
        /// Override a script's submenu
        /// </summary>
        /// <param name="qualifiedName">Script's qualified name</param>
        /// <param name="value">Submenu name</param>
        public void SetSubmenuOverride(string qualifiedName, string value)
        {
            _submenuOverrides[qualifiedName] = value;
            WriteConfig();
        }

        /// <summary>
        /// Remove a script's submenu override
        /// </summary>
        /// <param name="qualifiedName">Script's qualified name</param>
        /// <returns>True if it was removed</returns>
        public bool RemoveSubmenuOverride(string qualifiedName)
        {
            var removed = _submenuOverrides.Remove(qualifiedName);
            if (removed) WriteConfig();
            return removed;
        }

        /// <summary>
        /// Apply a keybind
        /// </summary>
        /// <param name="context">Context for the keybind</param>
        /// <param name="qualifiedName">Action's qualified name</param>
        /// <param name="value">Keybind</param>
        public void SetKeybind(KeybindContext context, string qualifiedName, string value)
        {
            switch (context)
            {
                case KeybindContext.GLOBAL:
                    _keybindsRegistry.GlobalBinds[qualifiedName] = value;
                    break;
                case KeybindContext.GRID:
                    _keybindsRegistry.GridBinds[qualifiedName] = value;
                    break;
                case KeybindContext.EDIT:
                    _keybindsRegistry.EditBinds[qualifiedName] = value;
                    break;
                case KeybindContext.AUDIO:
                    _keybindsRegistry.AudioBinds[qualifiedName] = value;
                    break;
                case KeybindContext.VIDEO:
                    _keybindsRegistry.VideoBinds[qualifiedName] = value;
                    break;
            }
            
            WriteKeybinds();
            OnPropertyChanged(nameof(KeybindsRegistry));
        }

        /// <summary>
        /// Set multiple keybinds. Empty string removes.
        /// </summary>
        /// <param name="context">Context for the keybind</param>
        /// <param name="keybinds">Map</param>
        public void SetKeybinds(KeybindContext context, Dictionary<string, string> keybinds)
        {
            Dictionary<string, string> binds = context switch
            {
                KeybindContext.GLOBAL => _keybindsRegistry.GlobalBinds,
                KeybindContext.GRID => _keybindsRegistry.GridBinds,
                KeybindContext.EDIT => _keybindsRegistry.EditBinds,
                KeybindContext.AUDIO => _keybindsRegistry.AudioBinds,
                KeybindContext.VIDEO => _keybindsRegistry.VideoBinds,
                _ => new Dictionary<string, string>()
            };
            foreach (var pair in keybinds)
            {
                if (pair.Value != string.Empty)
                    binds[pair.Key] = pair.Value;
                else
                    binds.Remove(pair.Key);
            }

            WriteKeybinds();
            OnPropertyChanged(nameof(KeybindsRegistry));
        }

        /// <summary>
        /// Remove a keybind
        /// </summary>
        /// <param name="context">Context for the keybind</param>
        /// <param name="qualifiedName">Action's qualified name</param>
        /// <returns>True if it was removed</returns>
        public bool RemoveKeybind(KeybindContext context, string qualifiedName)
        {
            var removed = context switch
            {
                KeybindContext.GLOBAL => _keybindsRegistry.GlobalBinds.Remove(qualifiedName),
                KeybindContext.GRID => _keybindsRegistry.GridBinds.Remove(qualifiedName),
                KeybindContext.EDIT => _keybindsRegistry.EditBinds.Remove(qualifiedName),
                KeybindContext.AUDIO => _keybindsRegistry.AudioBinds.Remove(qualifiedName),
                KeybindContext.VIDEO => _keybindsRegistry.VideoBinds.Remove(qualifiedName),
                _ => false
            };
            
            if (removed) WriteKeybinds();
            OnPropertyChanged(nameof(KeybindsRegistry));
            return removed;
        }

        /// <summary>
        /// Get a list of repositories
        /// </summary>
        /// <returns>List of repositories</returns>
        public List<string> GetRepositories()
        {
            return _repositories.ToList();
        }

        /// <summary>
        /// Get a list of submenu overrides
        /// </summary>
        /// <returns>List of Tuples containing the submenu overrides</returns>
        public List<Tuple<string, string>> GetSubmenuOverrides()
        {
            return _submenuOverrides.Select(k => new Tuple<string, string>(k.Key, k.Value)).ToList();
        }

        public Dictionary<string, string> SubmenuOverridesMap => new Dictionary<string, string>(_submenuOverrides);
        public Dictionary<string, List<string>> InstalledScriptsMap => new Dictionary<string, List<string>>(_installedScripts);
        public KeybindsRegistry KeybindsRegistry => _keybindsRegistry;

        public void ReadConfig()
        {
            if (!File.Exists(_configFilePath))
            {
                WriteConfig();
                return;
            }

            try
            {
                using var reader = new StreamReader(_configFilePath);
                var configContents = reader.ReadToEnd();
                FromConfigurationModel(TomletMain.To<ConfigurationModel>(configContents));
            }
            catch { throw new IOException($"An error occured while loading config file {_configFilePath}"); }
        }

        public void ReadKeybinds()
        {
            if (!File.Exists(_keybindsFilePath))
            {
                WriteKeybinds();
                return;
            }

            try
            {
                using var reader = new StreamReader(_keybindsFilePath);
                var keybindContents = reader.ReadToEnd();
                _keybindsRegistry = JsonSerializer.Deserialize<KeybindsRegistry>(keybindContents)!;
            }
            catch { throw new IOException($"An error occured while loading keybinds file {_keybindsFilePath}"); }
        }

        public async void WriteConfig()
        {
            try
            {
                using var configWriter = new StreamWriter(_configFilePath);
                string m = TomletMain.TomlStringFrom(ToConfigurationModel(), TomlSerializerOptions.Default);
                await configWriter.WriteAsync(m);
            } catch { HoloContext.Logger.Error("Failed to write config!", "ConfigurationManager"); return; }
        }

        public async void WriteKeybinds()
        {
            try
            {
                using var keybindsWriter = new StreamWriter(_keybindsFilePath);
                string kb = JsonSerializer.Serialize(_keybindsRegistry);
                await keybindsWriter.WriteAsync(kb);
            }
            catch { HoloContext.Logger.Error("Failed to write keybinds!", "ConfigurationManager"); return; }
        }

        private ConfigurationModel ToConfigurationModel()
        {
            return new ConfigurationModel
            {
                ConfigVersion = 1.0,
                AV = new AVConfigModel
                {
                    AudioProvider = this._audioProvider,
                    VideoProvider = this._videoProvider
                },
                General = new GeneralConfigModel
                {
                    Cps = this.Cps,
                    Autosave = this._autosave,
                    AutosaveInterval = this._autosaveInterval,
                    UseSoftLinebreaks = this._useSoftLinebreaks
                },
                DependencyControl = new DCConfigModel
                {
                    Repositories = this._repositories.ToList(),
                    SubmenuOverrides = this._submenuOverrides,
                    InstalledScripts = this._installedScripts
                }
            };
        }

        private void FromConfigurationModel(ConfigurationModel model)
        {
            this._audioProvider = model.AV?.AudioProvider ?? string.Empty;
            this._videoProvider = model.AV?.VideoProvider ?? string.Empty;
            this._cps = model.General?.Cps ?? 0;
            this._autosave = model.General?.Autosave ?? true;
            this._autosaveInterval = model.General?.AutosaveInterval ?? 300; // 5 minutes
            this._useSoftLinebreaks = model.General?.UseSoftLinebreaks ?? false;
            this._repositories = new ObservableCollection<string>(model.DependencyControl?.Repositories);
            this._submenuOverrides = new Dictionary<string, string>(model.DependencyControl?.SubmenuOverrides);
            this._installedScripts = new Dictionary<string, List<string>>(model.DependencyControl?.InstalledScripts);
        }

        public ConfigurationManager(string baseDirectory)
        {
            _configFilePath = Path.Join(baseDirectory, "config.toml");
            _keybindsFilePath = Path.Join(baseDirectory, "keybinds.json");
            _audioProvider = string.Empty;
            _videoProvider = string.Empty;
            _cps = 0;
            _autosave = true;
            _autosaveInterval = 300; // 5 minutes
            _repositories = new ObservableCollection<string>();
            _submenuOverrides = new Dictionary<string, string>();
            _installedScripts = new Dictionary<string, List<string>>();
            _keybindsRegistry = KeybindsRegistry.Default();
            ReadConfig();
            ReadKeybinds();
        }

        #region Models
        private class ConfigurationModel
        {
            public double? ConfigVersion;
            public AVConfigModel? AV;
            public GeneralConfigModel? General;
            public DCConfigModel? DependencyControl;
        }

        private class AVConfigModel
        {
            public string? AudioProvider;
            public string? VideoProvider;
        }

        private class GeneralConfigModel
        {
            public int? Cps;
            public bool? Autosave;
            public int? AutosaveInterval;
            public bool? UseSoftLinebreaks;
        }

        private class DCConfigModel
        {
            public List<string>? Repositories;
            public Dictionary<string, string>? SubmenuOverrides;
            public Dictionary<string, List<string>>? InstalledScripts;
        }
        #endregion Models

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        } 
    }

    public class KeybindsRegistry
    {
        public Dictionary<string, string> GlobalBinds { get; set; }
        public Dictionary<string, string> GridBinds { get; set; }
        public Dictionary<string, string> EditBinds { get; set; }
        public Dictionary<string, string> AudioBinds { get; set; }
        public Dictionary<string, string> VideoBinds { get; set; }

        public KeybindsRegistry()
        {
            GlobalBinds = new Dictionary<string, string>();
            GridBinds = new Dictionary<string, string>();
            EditBinds = new Dictionary<string, string>();
            AudioBinds = new Dictionary<string, string>();
            VideoBinds = new Dictionary<string, string>();
        }

        public static KeybindsRegistry Default()
        {
            return new KeybindsRegistry
            {
                GlobalBinds = new Dictionary<string, string>
                {
                    ["ameko.file.new"] = "Ctrl+N",
                    ["ameko.file.open"] = "Ctrl+O",
                    ["ameko.file.save"] = "Ctrl+S",
                    ["ameko.file.saveas"] = "Ctrl+Shift+S",
                    ["ameko.file.search"] = "Ctrl+F",
                    ["ameko.file.shift"] = "Ctrl+I",
                    ["ameko.file.undo"] = "Ctrl+Z",
                    ["ameko.file.redo"] = "Ctrl+Y",
                    ["ameko.video.jump"] = "Ctrl+G",
                    ["ameko.app.about"] = "Shift+F1",
                    ["ameko.app.quit"] = "Ctrl+Q"
                },
                GridBinds = new Dictionary<string, string>
                {
                    ["ameko.event.duplicate"] = "Ctrl+D",
                    ["ameko.event.copy"] = "Ctrl+C",
                    ["ameko.event.cut"] = "Ctrl+X",
                    ["ameko.event.paste"] = "Ctrl+V",
                    ["ameko.event.pasteover"] = "Ctrl+Shift+V",
                    ["ameko.event.delete"] = "Shift+Delete"
                }
            };
        }

        public static List<string> GetBuiltins(KeybindContext context)
        {
            var def = Default();
            return context switch
            {
                KeybindContext.GLOBAL => def.GlobalBinds.Keys.ToList(),
                KeybindContext.GRID => def.GridBinds.Keys.ToList(),
                KeybindContext.EDIT => def.EditBinds.Keys.ToList(),
                KeybindContext.AUDIO => def.AudioBinds.Keys.ToList(),
                KeybindContext.VIDEO => def.VideoBinds.Keys.ToList(),
                _ => new List<string>()
            };
        }
    }

    public enum KeybindContext
    {
        GLOBAL,
        GRID,
        EDIT,
        AUDIO,
        VIDEO
    }

}

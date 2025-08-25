// SPDX-License-Identifier: MPL-2.0

using System.Collections.Concurrent;
using System.IO.Abstractions;
using System.Text.Json;
using System.Text.Json.Serialization;
using Holo.IO;
using NLog;

namespace Holo.Configuration.Keybinds;

public class KeybindRegistrar(IFileSystem fileSystem) : IKeybindRegistrar
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
    };

    /// <summary>
    /// The filesystem being used
    /// </summary>
    private readonly IFileSystem _fileSystem = fileSystem;

    /// <summary>
    /// Keybind map
    /// </summary>
    private readonly ConcurrentDictionary<string, Keybind> _keybinds = new();

    /// <inheritdoc />
    public bool RegisterKeybind(Keybind keybind)
    {
        var result = _keybinds.TryAdd(keybind.QualifiedName, keybind);
        if (result)
            OnKeybindsChanged?.Invoke(this, EventArgs.Empty);
        return result;
    }

    /// <inheritdoc />
    public bool RegisterKeybinds(IList<Keybind> keybinds, bool save)
    {
        var changed = false;
        foreach (var keybind in keybinds)
        {
            if (_keybinds.TryAdd(keybind.QualifiedName, keybind))
                changed = true;
        }
        OnKeybindsChanged?.Invoke(this, EventArgs.Empty);

        if (save && changed)
            Save();
        return true;
    }

    /// <inheritdoc />
    public bool DeregisterKeybind(string qualifiedName)
    {
        var result = _keybinds.TryRemove(qualifiedName, out _);
        if (result)
            OnKeybindsChanged?.Invoke(this, EventArgs.Empty);
        return result;
    }

    /// <inheritdoc />
    public bool ApplyOverride(string qualifiedName, string? keybind, KeybindContext? context)
    {
        if (!_keybinds.TryGetValue(qualifiedName, out var target))
        {
            Logger.Error($"Could not find keybind {qualifiedName}");
            return false;
        }

        var changed = false;

        if (keybind != target.OverrideKey)
        {
            changed = true;
            target.OverrideKey = keybind;
        }

        if (context is not null && context != target.Context)
        {
            changed = true;
            target.Context = context.Value;
        }

        if (changed)
            OnKeybindsChanged?.Invoke(this, EventArgs.Empty);
        return true;
    }

    /// <inheritdoc />
    public bool ClearOverride(string qualifiedName)
    {
        if (!_keybinds.TryGetValue(qualifiedName, out var target))
        {
            Logger.Error($"Could not find keybind {qualifiedName}");
            return false;
        }

        target.OverrideKey = null;
        OnKeybindsChanged?.Invoke(this, EventArgs.Empty);
        return true;
    }

    /// <inheritdoc />
    public Keybind? GetKeybind(string qualifiedName)
    {
        return _keybinds.GetValueOrDefault(qualifiedName);
    }

    /// <inheritdoc />
    public bool TryGetKeybind(string qualifiedName, out Keybind? keybind)
    {
        keybind = _keybinds.GetValueOrDefault(qualifiedName);
        return keybind is not null;
    }

    /// <inheritdoc />
    public bool IsKeybindRegistered(string qualifiedName)
    {
        return _keybinds.ContainsKey(qualifiedName);
    }

    /// <inheritdoc />
    public IEnumerable<Keybind> GetKeybinds()
    {
        return _keybinds.Values;
    }

    /// <inheritdoc />
    public IEnumerable<Keybind> GetKeybinds(KeybindContext filter)
    {
        return _keybinds.Values.Where(keybind => (keybind.Context & filter) != 0);
    }

    /// <inheritdoc />
    public IEnumerable<Keybind> GetOverridenKeybinds()
    {
        return _keybinds.Values.Where(keybind =>
            !keybind.IsEnabled || keybind.OverrideKey is not null
        );
    }

    /// <inheritdoc />
    public void Save()
    {
        var path = Paths.Keybinds.LocalPath;
        Logger.Info($"Saving keybinds to {path}...");
        try
        {
            if (!_fileSystem.Directory.Exists(Path.GetDirectoryName(path)))
                _fileSystem.Directory.CreateDirectory(Path.GetDirectoryName(path) ?? "/");

            using var fs = _fileSystem.FileStream.New(
                path,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None
            );
            using var writer = new StreamWriter(fs);

            var content = JsonSerializer.Serialize(_keybinds, JsonOptions);
            writer.Write(content);
            Logger.Info("Done!");
        }
        catch (Exception ex) when (ex is IOException or JsonException)
        {
            Logger.Error(ex);
        }
    }

    /// <inheritdoc />
    public void Parse()
    {
        Logger.Info("Parsing keybinds...");
        var path = Paths.Keybinds.LocalPath;
        try
        {
            if (!_fileSystem.Directory.Exists(Path.GetDirectoryName(path)))
                _fileSystem.Directory.CreateDirectory(Path.GetDirectoryName(path) ?? "/");

            if (!_fileSystem.File.Exists(path))
            {
                Logger.Warn("Keybinds file does not exist, using defaults...");
                Save();
                return;
            }

            using var fs = _fileSystem.FileStream.New(
                path,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite
            );
            using var reader = new StreamReader(fs);

            var imports = JsonSerializer.Deserialize<Dictionary<string, Keybind>>(
                reader.ReadToEnd(),
                JsonOptions
            );

            if (imports is null)
            {
                Logger.Error("Failed to parse keybinds, using defaults...");
                return;
            }

            foreach (var import in imports)
            {
                import.Value.QualifiedName = import.Key;

                if (_keybinds.TryGetValue(import.Key, out var target))
                {
                    if (import.Value.OverrideKey is not null)
                        target.OverrideKey = import.Value.OverrideKey;
                    target.Context = import.Value.Context;
                }
                else
                {
                    _keybinds.TryAdd(import.Key, import.Value);
                }
            }
            Logger.Info("Done!");
        }
        catch (Exception ex) when (ex is IOException or JsonException)
        {
            Logger.Error(ex);
            Logger.Error("Failed to parse keybinds, using defaults...");
        }
        finally
        {
            OnKeybindsChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <inheritdoc />
    public event EventHandler<EventArgs>? OnKeybindsChanged;
}

// SPDX-License-Identifier: MPL-2.0

using System.Collections.Concurrent;
using NLog;

namespace Holo.Configuration.Keybinds;

public class KeybindRegistrar : IKeybindRegistrar
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

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
    public bool RegisterKeybinds(IList<Keybind> keybinds)
    {
        var result = keybinds.All(keybind => _keybinds.TryAdd(keybind.QualifiedName, keybind));
        if (result)
            OnKeybindsChanged?.Invoke(this, EventArgs.Empty);
        return result;
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

        if (keybind != target.OverrideKey)
            target.OverrideKey = keybind;

        if (context != target.OverrideContext)
            target.OverrideContext = context;

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
        target.OverrideContext = null;
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
    public IEnumerable<Keybind> GetKeybinds(KeybindContext filter)
    {
        return _keybinds.Values.Where(keybind => (keybind.DefaultContext & filter) != 0);
    }

    /// <inheritdoc />
    public IEnumerable<Keybind> GetOverridenKeybinds()
    {
        return _keybinds.Values.Where(keybind =>
            !keybind.IsEnabled
            || keybind.OverrideKey is not null
            || keybind.OverrideContext is not null
        );
    }

    /// <inheritdoc />
    public event IKeybindRegistrar.KeybindsChangedEventHandler? OnKeybindsChanged;
}

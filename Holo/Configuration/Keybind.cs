// SPDX-License-Identifier: MPL-2.0

namespace Holo.Configuration;

/// <summary>
/// Represents a registered keybind
/// </summary>
/// <param name="qualifiedName">Unique accessor for the action the keybind is assigned to</param>
/// <param name="defaultKey">Key combination</param>
/// <param name="defaultContext">Contexts the keybind is registered to by default</param>
public class Keybind(string qualifiedName, string defaultKey, KeybindContext defaultContext)
{
    /// <summary>
    /// Unique accessor for the action the keybind is assigned to
    /// </summary>
    /// <remarks>
    /// All built-in actions are namespaced, starting with "ameko".
    /// Script keybinds are accessed using the qualified name of themselves or
    /// of their exported methods.
    /// </remarks>
    /// <example><c>ameko.document.save</c></example>
    public string QualifiedName { get; } = qualifiedName;

    /// <summary>
    /// Default key combination
    /// </summary>
    /// <example><c>Ctrl+Shift+S</c></example>
    public string DefaultKey { get; } = defaultKey;

    /// <summary>
    /// User-set key combination
    /// </summary>
    /// <example><c>Ctrl+Shift+S</c></example>
    public string? OverrideKey { get; internal set; }

    /// <summary>
    /// Effective key combination
    /// </summary>
    /// <returns>
    /// <see cref="OverrideKey"/> if set, otherwise <see cref="DefaultKey"/>
    /// </returns>
    public string Key => OverrideKey ?? DefaultKey;

    /// <summary>
    /// The contexts the keybind is registered to by default
    /// </summary>
    /// <example><c>var contexts = KeybindContext.Editor | KeybindContext.Grid;</c></example>
    public KeybindContext DefaultContext { get; } = defaultContext;

    /// <summary>
    /// User-set contexts
    /// </summary>
    public KeybindContext? OverrideContext { get; internal set; }

    /// <summary>
    /// Effective contexts
    /// </summary>
    /// <returns>
    /// <see cref="OverrideContext"/> if set, otherwise <see cref="DefaultContext"/>
    /// </returns>
    public KeybindContext Context => OverrideContext ?? DefaultContext;

    /// <summary>
    /// If the keybind is enabled
    /// </summary>
    public bool IsEnabled { get; internal set; }
}

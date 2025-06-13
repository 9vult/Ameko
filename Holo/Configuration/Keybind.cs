// SPDX-License-Identifier: MPL-2.0

namespace Holo.Configuration;

/// <summary>
/// Represents a registered keybind
/// </summary>
/// <param name="qualifiedName">Unique accessor for the action the keybind is assigned to</param>
/// <param name="key">Key combination</param>
/// <param name="context">Contexts the keybind is registered to</param>
public class Keybind(string qualifiedName, string key, KeybindContext context)
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
    /// Key combination
    /// </summary>
    /// <example><c>Ctrl+Shift+S</c></example>
    public string Key { get; } = key;

    /// <summary>
    /// The contexts the keybind is registered to
    /// </summary>
    /// <example><c>var contexts = KeybindContext.Editor | KeybindContext.Grid;</c></example>
    public KeybindContext Context { get; } = context;
}

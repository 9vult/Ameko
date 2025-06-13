// SPDX-License-Identifier: MPL-2.0

namespace Holo.Configuration;

/// <summary>
/// Provides methods for registering, retrieving,
/// and managing keybinds within specific contexts
/// </summary>
public interface IKeybindRegistrar
{
    /// <summary>
    /// Registers a new keybind or replaces an existing one with the same qualified name
    /// </summary>
    /// <param name="keybind">The <see cref="Keybind"/> to register</param>
    void RegisterKeybind(Keybind keybind);

    /// <summary>
    /// Removes the keybind associated with the specified keybind name
    /// </summary>
    /// <param name="qualifiedName">The keybind name</param>
    void DeregisterKeybind(string qualifiedName);

    /// <summary>
    /// Retrieves the keybind registered under the specified qualified name.
    /// </summary>
    /// <param name="qualifiedName">The action name to look up</param>
    /// <returns>The associated <see cref="Keybind"/>, or <see langword="null" /> if not found</returns>
    Keybind? GetKeybind(string qualifiedName);

    /// <summary>
    /// Attempts to retrieve the keybind registered under the specified keybind name
    /// </summary>
    /// <param name="qualifiedName">The action name to look up</param>
    /// <param name="keybind">The keybind, if found, otherwise, <see langword="null" /></param>
    /// <returns><see langword="true" /> if the keybind was found</returns>
    bool TryGetKeybind(string qualifiedName, out Keybind keybind);

    /// <summary>
    /// Determines whether a keybind is registered under the specified qualified name.
    /// </summary>
    /// <param name="qualifiedName">The keybind name to check</param>
    /// <returns><see langword="true" /> if a keybind is registered to the given name</returns>
    bool IsKeybindRegistered(string qualifiedName);

    /// <summary>
    /// Retrieves all keybinds that match the specified context filter
    /// </summary>
    /// <param name="filter">A combination of <see cref="KeybindContext"/> flags used to filter keybinds</param>
    /// <returns>Keybinds that are active in any of the specified contexts</returns>
    IEnumerable<Keybind> GetKeybinds(KeybindContext filter);
}

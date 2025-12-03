// SPDX-License-Identifier: MPL-2.0

namespace Holo.Configuration.Keybinds;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Method)]
public sealed class KeybindTargetAttribute : Attribute
{
    public string QualifiedName { get; }
    public string? DefaultKey { get; }
    public KeybindContext DefaultContext { get; }

    /// <summary>
    /// Denotes a keybind target with a default keybind assigned
    /// </summary>
    /// <param name="qualifiedName">Keybind identifier</param>
    /// <param name="defaultKey">Default keybind</param>
    /// <param name="defaultContext">Default keybind context</param>
    public KeybindTargetAttribute(
        string qualifiedName,
        string defaultKey,
        KeybindContext defaultContext
    )
    {
        QualifiedName = qualifiedName;
        DefaultKey = defaultKey;
        DefaultContext = defaultContext;
    }

    /// <summary>
    /// Denotes a keybind target without a default keybind assigned
    /// </summary>
    /// <param name="qualifiedName">Keybind identifier</param>
    /// <param name="defaultContext">Default keybind context</param>
    public KeybindTargetAttribute(string qualifiedName, KeybindContext defaultContext)
    {
        QualifiedName = qualifiedName;
        DefaultContext = defaultContext;
    }
}

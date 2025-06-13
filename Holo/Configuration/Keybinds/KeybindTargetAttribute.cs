// SPDX-License-Identifier: MPL-2.0

namespace Holo.Configuration.Keybinds;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Method)]
public sealed class KeybindTargetAttribute : Attribute
{
    public string QualifiedName { get; }
    public string? DefaultKey { get; }
    public KeybindContext? DefaultContext { get; }

    public KeybindTargetAttribute(string qualifiedName, string? defaultKey = null)
    {
        QualifiedName = qualifiedName;
        DefaultKey = defaultKey;
    }

    public KeybindTargetAttribute(
        string qualifiedName,
        string? defaultKey,
        KeybindContext defaultContext
    )
    {
        QualifiedName = qualifiedName;
        DefaultKey = defaultKey;
        DefaultContext = defaultContext;
    }
}

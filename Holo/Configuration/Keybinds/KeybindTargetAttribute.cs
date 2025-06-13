// SPDX-License-Identifier: MPL-2.0

namespace Holo.Configuration.Keybinds;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Method)]
public sealed class KeybindTargetAttribute(
    string qualifiedName,
    string? defaultKey = null,
    KeybindContext? defaultContext = null
) : Attribute
{
    public string QualifiedName { get; } = qualifiedName;
    public string? DefaultKey { get; } = defaultKey;
    public KeybindContext? DefaultContext { get; } = defaultContext;
}

// SPDX-License-Identifier: MPL-2.0

namespace Holo.Configuration.Keybinds;

[AttributeUsage(AttributeTargets.Class)]
public sealed class KeybindContextAttribute(KeybindContext defaultContext) : Attribute
{
    public KeybindContext DefaultContext { get; } = defaultContext;
}

// SPDX-License-Identifier: GPL-3.0-only

using Holo.Configuration.Keybinds;

namespace Ameko.DataModels;

public class EditableKeybind
{
    public required string QualifiedName { get; init; }
    public required string DefaultKey { get; init; }
    public required string? OverrideKey { get; set; }
    public required KeybindContext DefaultContext { get; init; }
    public required KeybindContext? OverrideContext { get; set; }

    public string Key
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(OverrideKey))
                return OverrideKey;
            if (!string.IsNullOrWhiteSpace(DefaultKey))
                return DefaultKey;
            return string.Empty;
        }
        set => OverrideKey = value != DefaultKey ? value : null;
    }

    public KeybindContext? Context
    {
        get => OverrideContext ?? DefaultContext;
        set => OverrideContext = value != DefaultContext ? value : null;
    }
}

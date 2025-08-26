// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.Collections.ObjectModel;
using Holo.Configuration.Keybinds;

namespace Ameko.DataModels;

public class EditableKeybind
{
    public required bool IsBuiltin { get; init; }
    public required string QualifiedName { get; init; }
    public required string DefaultKey { get; init; }
    public required string? OverrideKey { get; set; }
    public required EditableKeybindContext Context { get; set; }

    public static ReadOnlyCollection<EditableKeybindContext> Contexts { get; } =
        new(
            [
                new EditableKeybindContext(KeybindContext.None),
                new EditableKeybindContext(KeybindContext.Global),
                new EditableKeybindContext(KeybindContext.Grid),
                new EditableKeybindContext(KeybindContext.Editor),
                new EditableKeybindContext(KeybindContext.Video),
                new EditableKeybindContext(KeybindContext.Audio),
            ]
        );

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
}

public class EditableKeybindContext
{
    public KeybindContext Context { get; init; }
    public string Display { get; init; }

    public EditableKeybindContext(KeybindContext context)
    {
        Context = context;
        Display = context switch
        {
            KeybindContext.None => I18N.Keybinds.KeybindContext_None,
            KeybindContext.Global => I18N.Keybinds.KeybindContext_Global,
            KeybindContext.Grid => I18N.Keybinds.KeybindContext_Grid,
            KeybindContext.Editor => I18N.Keybinds.KeybindContext_Editor,
            KeybindContext.Video => I18N.Keybinds.KeybindContext_Video,
            KeybindContext.Audio => I18N.Keybinds.KeybindContext_Audio,
            _ => throw new ArgumentOutOfRangeException(nameof(context), context, null),
        };
    }

    protected bool Equals(EditableKeybindContext other)
    {
        return Context == other.Context;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return Display;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        if (obj is null)
            return false;
        if (ReferenceEquals(this, obj))
            return true;
        if (obj.GetType() != GetType())
            return false;
        return Equals((EditableKeybindContext)obj);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return (int)Context;
    }
}

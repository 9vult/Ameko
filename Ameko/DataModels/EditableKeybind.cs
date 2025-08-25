// SPDX-License-Identifier: GPL-3.0-only

using Holo.Configuration.Keybinds;

namespace Ameko.DataModels;

public class EditableKeybind
{
    public required bool IsBuiltin { get; init; }
    public required string QualifiedName { get; init; }
    public required string DefaultKey { get; init; }
    public required string? OverrideKey { get; set; }
    public required EditableKeybindContext Context { get; set; }

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
    public required KeybindContext Context { get; init; }
    public required string Display { get; init; }

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

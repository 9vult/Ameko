// SPDX-License-Identifier: MPL-2.0

namespace Holo.Configuration;

/// <summary>
/// Represents the context a keybind is registered to
/// </summary>
[Flags]
public enum KeybindContext
{
    /// <summary>
    /// The keybind is not registered
    /// </summary>
    None = 0b00000,

    /// <summary>
    /// The keybind is registered for use everywhere
    /// </summary>
    Global = 0b00001,

    /// <summary>
    /// The keybind is registered for use in the subtitle grid
    /// </summary>
    Grid = 0b00010,

    /// <summary>
    /// The keybind is registered for use in the editing area
    /// </summary>
    Editor = 0b00100,

    /// <summary>
    /// The keybind is registered for use in the audio area
    /// </summary>
    Audio = 0b01000,

    /// <summary>
    /// The keybind is registered for use in the video area
    /// </summary>
    Video = 0b10000,
}

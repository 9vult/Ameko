// SPDX-License-Identifier: MPL-2.0

namespace Holo.Scripting.Models;

/// <summary>
/// Type of <see cref="Module"/>
/// </summary>
public enum ModuleType
{
    /// <summary>
    /// The <see cref="Module"/> is an executable <see cref="HoloScript"/>
    /// </summary>
    Script = 0,

    /// <summary>
    /// The <see cref="Module"/> is a <see cref="HoloLibrary"/>
    /// </summary>
    Library = 1,

    /// <summary>
    /// The <see cref="Module"/> is a <see cref="HoloScriptlet"/>
    /// </summary>
    Scriptlet = 2,
}

// SPDX-License-Identifier: MPL-2.0

using Xdg.Directories;

namespace Holo.IO;

/// <summary>
/// Manages the XDG directories used by Holo and Ameko
/// </summary>
public static class Directories
{
    public static string DataHome => Path.Combine(BaseDirectory.DataHome, "Ameko");
    public static string ConfigHome => Path.Combine(BaseDirectory.ConfigHome, "Ameko");
    public static string CacheHome => Path.Combine(BaseDirectory.CacheHome, "Ameko");
    public static string StateHome => Path.Combine(BaseDirectory.StateHome, "Ameko");

    /// <summary>
    /// Create the XDG directories if they do not already exist
    /// </summary>
    internal static void Create()
    {
        if (!Directory.Exists(DataHome))
            Directory.CreateDirectory(DataHome);
        if (!Directory.Exists(ConfigHome))
            Directory.CreateDirectory(ConfigHome);
        if (!Directory.Exists(CacheHome))
            Directory.CreateDirectory(CacheHome);
        if (Directory.Exists(StateHome))
            Directory.CreateDirectory(StateHome);
    }
}

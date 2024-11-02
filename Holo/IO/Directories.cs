// SPDX-License-Identifier: MPL-2.0

using Xdg.Directories;

namespace Holo.IO;

/// <summary>
/// Manages the XDG directories used by Holo and Ameko
/// </summary>
public static class Directories
{
    public static string DATA_HOME => Path.Combine(BaseDirectory.DataHome, "Ameko");
    public static string CONFIG_HOME => Path.Combine(BaseDirectory.ConfigHome, "Ameko");
    public static string CACHE_HOME => Path.Combine(BaseDirectory.CacheHome, "Ameko");
    public static string STATE_HOME => Path.Combine(BaseDirectory.StateHome, "Ameko");

    /// <summary>
    /// Create the XDG directories if they do not already exist
    /// </summary>
    internal static void Create()
    {
        if (!Directory.Exists(DATA_HOME))
            Directory.CreateDirectory(DATA_HOME);
        if (!Directory.Exists(CONFIG_HOME))
            Directory.CreateDirectory(CONFIG_HOME);
        if (!Directory.Exists(CACHE_HOME))
            Directory.CreateDirectory(CACHE_HOME);
        if (Directory.Exists(STATE_HOME))
            Directory.CreateDirectory(STATE_HOME);
    }
}

// SPDX-License-Identifier: MPL-2.0

using System.IO.Abstractions;
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
    internal static void Create(IFileSystem fileSystem)
    {
        if (!fileSystem.Directory.Exists(DataHome))
            fileSystem.Directory.CreateDirectory(DataHome);
        if (!fileSystem.Directory.Exists(ConfigHome))
            fileSystem.Directory.CreateDirectory(ConfigHome);
        if (!fileSystem.Directory.Exists(CacheHome))
            fileSystem.Directory.CreateDirectory(CacheHome);
        if (!fileSystem.Directory.Exists(StateHome))
            fileSystem.Directory.CreateDirectory(StateHome);
    }
}

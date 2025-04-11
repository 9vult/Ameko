// SPDX-License-Identifier: MPL-2.0

namespace Holo.IO;

public static class Paths
{
    /// <summary>
    /// Default file location for the application configuration
    /// </summary>
    public static Uri Configuration => new(Path.Combine(Directories.ConfigHome, "config.json"));
}

// SPDX-License-Identifier: MPL-2.0

namespace Holo.IO;

public static class Paths
{
    /// <summary>
    /// Default file location for the application configuration
    /// </summary>
    public static Uri Configuration =>
        GetOrCreate(Path.Combine(Directories.ConfigHome, "config.json"));

    /// <summary>
    /// Checks if a file exists at <paramref name="path"/>
    /// and creates one if not.
    /// </summary>
    /// <param name="path">Filepath</param>
    /// <returns><see cref="Uri"/> for the <paramref name="path"/>.</returns>
    private static Uri GetOrCreate(string path)
    {
        if (!File.Exists(path))
            File.Create(path);
        return new Uri(path);
    }
}

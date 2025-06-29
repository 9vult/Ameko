﻿// SPDX-License-Identifier: MPL-2.0

namespace Holo.IO;

public static class Paths
{
    /// <summary>
    /// Default file location for the application configuration
    /// </summary>
    public static Uri Configuration =>
        new(Path.Combine(DirectoryService.ConfigHome, "config.json"));

    /// <summary>
    /// Default file location for application persistence
    /// </summary>
    public static Uri Persistence => new(Path.Combine(DirectoryService.ConfigHome, "persist.json"));

    /// <summary>
    /// Default file location for globally-accessible objects
    /// </summary>
    public static Uri Globals => new(Path.Combine(DirectoryService.ConfigHome, "globals.json"));

    /// <summary>
    /// Default file location for keybinds
    /// </summary>
    public static Uri Keybinds => new(Path.Combine(DirectoryService.ConfigHome, "keybinds.json"));
}

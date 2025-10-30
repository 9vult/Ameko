// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.Diagnostics;

namespace Ameko.Services;

public static class SystemService
{
    /// <summary>
    /// Get the current desktop environment
    /// </summary>
    public static string DesktopEnvironment
    {
        get
        {
            if (OperatingSystem.IsWindows())
                return "Aero";
            if (OperatingSystem.IsMacOS())
                return "Aqua";
            if (!OperatingSystem.IsLinux())
                return "Unknown";

            var xdg = Environment.GetEnvironmentVariable("XDG_CURRENT_DESKTOP");
            if (!string.IsNullOrEmpty(xdg))
            {
                return xdg == "KDE" ? "KDE Plasma" : xdg;
            }

            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("KDE_FULL_SESSION")))
                return "KDE Plasma";

            if (
                !string.IsNullOrEmpty(
                    Environment.GetEnvironmentVariable("GNOME_DESKTOP_SESSION_ID")
                )
            )
                return "GNOME";

            return "Unknown";
        }
    }

    /// <summary>
    /// Get the current window manager
    /// </summary>
    public static string WindowManager
    {
        get
        {
            if (OperatingSystem.IsWindows())
                return "Explorer";
            if (OperatingSystem.IsMacOS())
                return "Quartz Compositor";
            if (!OperatingSystem.IsLinux())
                return "Unknown";

            var xdg = Environment.GetEnvironmentVariable("XDG_SESSION_TYPE");
            if (!string.IsNullOrEmpty(xdg))
                return xdg;

            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WAYLAND_DISPLAY")))
                return "wayland";

            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DISPLAY")))
                return "x11";

            return "Unknown";
        }
    }

    /// <summary>
    /// Get the current platform
    /// </summary>
    public static string Platform
    {
        get
        {
            if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
            {
                var proc = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "uname",
                        Arguments = "-sr",
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                    },
                };

                try
                {
                    proc.Start();
                    var output = proc.StandardOutput.ReadToEnd().Trim();
                    proc.WaitForExit();
                    return output;
                }
                catch
                {
                    return "Unknown";
                }
            }
            return Environment.OSVersion.ToString();
        }
    }
}

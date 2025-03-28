// SPDX-License-Identifier: GPL-3.0-only

using System.Diagnostics.CodeAnalysis;

namespace Ameko.Services;

[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public static class VersionService
{
    public static string Major => ThisAssembly.Git.SemVer.Major;
    public static string Minor => ThisAssembly.Git.SemVer.Minor;
    public static string Patch => ThisAssembly.Git.SemVer.Patch;
    public static string DashLabel => ThisAssembly.Git.SemVer.DashLabel;
    public static string Position => $"{ThisAssembly.Git.Branch}-{ThisAssembly.Git.Commit}";

    /// <summary>
    /// Full version label: Major.Minor.Patch-Label @ Branch-ShortSha
    /// </summary>
    public static string FullLabel =>
        !string.IsNullOrEmpty(Major)
            ? $"{Major}.{Minor}.{Patch}{DashLabel} @ {Position}"
            : string.Empty;
}

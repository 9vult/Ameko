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
    public static string Branch => ThisAssembly.Git.Branch;
    public static string ShortSha => ThisAssembly.Git.Commit;

    /// <summary>
    /// Full version label: Major.Minor.Patch-Label+Branch.ShortSha
    /// </summary>
    /// <remarks>Follows proper SemVer 2.0 protocol</remarks>
    /// <seealso href="https://semver.org/"/>
    public static string FullLabel =>
        !string.IsNullOrEmpty(Major)
            ? $"{Major}.{Minor}.{Patch}{DashLabel}+{Branch}.{ShortSha}"
            : string.Empty;
}

// SPDX-License-Identifier: MPL-2.0

namespace Holo.Models;

/// <summary>
/// Simplified representation of a <see cref="Solution"/> for
/// serialization purposes
/// </summary>
internal record SolutionModel
{
    internal const decimal CURRENT_API_VERSION = 1.0m;

    public required decimal Version;
    public required List<string> ReferencedFiles;
    public required List<string> Styles;
    public required int Cps;
    public required bool? UseSoftLinebreaks;
}

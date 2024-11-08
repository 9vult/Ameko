// SPDX-License-Identifier: MPL-2.0

using Tomlet.Attributes;

namespace Holo.Models;

/// <summary>
/// Simplified representation of a <see cref="Solution"/> for
/// serialization purposes
/// </summary>
internal record SolutionModel
{
    [TomlNonSerialized]
    internal const double CURRENT_API_VERSION = 1.0d;

    public required double Version;
    public required List<string> ReferencedDocuments;
    public required List<string> Styles;
    public required int Cps;
    public required bool? UseSoftLinebreaks;
}

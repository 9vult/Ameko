// SPDX-License-Identifier: MPL-2.0

using System.Text.Json.Serialization;
using Holo.Configuration;

namespace Holo.Models;

/// <summary>
/// Simplified representation of a <see cref="Project"/> for
/// serialization purposes
/// </summary>
internal record ProjectModel
{
    [JsonIgnore]
    internal const double CurrentApiVersion = 1.0d;

    public required double Version;
    public required ProjectItemModel[] ReferencedDocuments;
    public required string[] Styles;
    public required uint? Cps;
    public required bool? CpsIncludesWhitespace;
    public required bool? CpsIncludesPunctuation;
    public required bool? UseSoftLinebreaks;
    public required int? DefaultLayer;
    public required string? SpellcheckCulture;
    public required string[] CustomWords;
    public required TimingButlerModel TimingButler;
}

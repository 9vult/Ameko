// SPDX-License-Identifier: MPL-2.0

using System.Collections.Frozen;
using System.Text.Json.Serialization;

namespace Holo.Scripting.Models;

/// <summary>
/// A Module is an entry in a <see cref="Repository"/>
/// for a <see cref="Scripting.HoloScript"/> or <see cref="Scripting.HoloLibrary"/>
/// </summary>
public record Module
{
    /// <summary>
    /// Type of module
    /// </summary>
    public required ModuleType Type { get; init; }

    /// <summary>
    /// Name used for display purposes
    /// </summary>
    /// <example><c>Encode Clip</c></example>
    public required string DisplayName { get; init; }

    /// <summary>
    /// Uniquely identifying name
    /// </summary>
    /// <remarks>
    /// The most common format is <c>author.scriptName</c>, but this isn't a requirement.
    /// Must match the <see cref="ModuleInfo.QualifiedName"/>
    /// defined in the module's <see cref="ModuleInfo"/>.
    /// </remarks>
    /// <example><c>petzku.encodeClip</c></example>
    public required string QualifiedName { get; init; }

    /// <summary>
    /// Short description of the module
    /// </summary>
    /// <example>"Encode a hardsubbed clip encompassing the current selection"</example>
    public required string Description { get; init; }

    /// <summary>
    /// Name of the author
    /// </summary>
    /// <example><c>petzku</c></example>
    public required string Author { get; init; }

    /// <summary>
    /// Current module version. (Major.Minor)
    /// </summary>
    /// <remarks>
    /// Must match the <see cref="ModuleInfo.Version"/>
    /// defined in the module's <see cref="ModuleInfo"/>.
    /// </remarks>
    public required decimal Version { get; init; }

    /// <summary>
    /// <see langword="true"/> if this is a beta module
    /// </summary>
    public required bool IsBetaChannel { get; init; }

    /// <summary>
    /// List of qualified module names this module depends on
    /// </summary>
    [JsonIgnore]
    public required FrozenSet<string> Dependencies { get; init; }

    /// <summary>
    /// List of tags for categorization
    /// </summary>
    [JsonIgnore]
    public required FrozenSet<string> Tags { get; init; }

    /// <summary>
    /// URL to download
    /// </summary>
    /// <example>https://dc.ameko.moe/scripts/9volt/9volt.example1.cs</example>
    public required string Url { get; set; }

    /// <summary>
    /// Name of repository (automatic)
    /// </summary>
    public string? Repository { get; set; }

    #region Serialization fields

    [JsonPropertyName("Dependencies")]
    public HashSet<string> SerializationDependencies
    {
        get => Dependencies.ToHashSet();
        init => Dependencies = value.ToFrozenSet();
    }

    [JsonPropertyName("Tags")]
    public HashSet<string> SerializationTags
    {
        get => Tags.ToHashSet();
        init => Tags = value.ToFrozenSet();
    }

    #endregion Serialization fields
}

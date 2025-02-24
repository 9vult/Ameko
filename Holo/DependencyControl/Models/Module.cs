// SPDX-License-Identifier: MPL-2.0

namespace Holo.DependencyControl.Models;

/// <summary>
/// A Module is an entry in a <see cref="Repository"/> for a <see cref="Scripting.HoloScript"/>
/// </summary>
public record Module
{
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
    /// Must match the <see cref="Scripting.ScriptInfo.QualifiedName"/>
    /// defined in the module's <see cref="Scripting.ScriptInfo"/>.
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
    /// Must match the <see cref="Scripting.ScriptInfo.Version"/>
    /// defined in the module's <see cref="Scripting.ScriptInfo"/>.
    /// </remarks>
    public required decimal Version { get; init; }

    /// <summary>
    /// <see langword="true"/> if this is a beta module
    /// </summary>
    public required bool IsBetaChannel { get; init; }

    /// <summary>
    /// List of qualified module names this module depends on
    /// </summary>
    public required List<string> Dependencies { get; init; }

    /// <summary>
    /// List of tags for categorization
    /// </summary>
    public required List<string> Tags { get; init; }

    /// <summary>
    /// URL to download
    /// </summary>
    /// <example>https://dc.ameko.moe/scripts/9volt/9volt.example1.cs</example>
    public required string Url { get; set; }

    /// <summary>
    /// Automatic property
    /// </summary>
    public string? Repository { get; set; }
}

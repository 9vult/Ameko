// SPDX-License-Identifier: MPL-2.0

using System.Collections.Frozen;
using System.Text;
using System.Text.Json.Serialization;

namespace Holo.Scripting.Models;

/// <summary>
/// A Package is an entry in a <see cref="Repository"/>
/// for a <see cref="Scripting.HoloScript"/> or <see cref="Scripting.HoloLibrary"/>
/// </summary>
public record Package
{
    /// <summary>
    /// Type of package
    /// </summary>
    public required PackageType Type { get; init; }

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
    /// Must match the <see cref="PackageInfo.QualifiedName"/>
    /// defined in the package's <see cref="PackageInfo"/>.
    /// </remarks>
    /// <example><c>petzku.encodeClip</c></example>
    public required string QualifiedName { get; init; }

    /// <summary>
    /// Short description of the package
    /// </summary>
    /// <example>"Encode a hardsubbed clip encompassing the current selection"</example>
    public required string Description { get; init; }

    /// <summary>
    /// Name of the author
    /// </summary>
    /// <example><c>petzku</c></example>
    public required string Author { get; init; }

    /// <summary>
    /// Current package version. (Major.Minor)
    /// </summary>
    public required decimal Version { get; init; }

    /// <summary>
    /// <see langword="true"/> if this is a beta package
    /// </summary>
    public required bool IsBetaChannel { get; init; }

    /// <summary>
    /// List of changelog entries
    /// </summary>
    public required Changelog[] Changelog { get; init; }

    /// <summary>
    /// If the package has a changelog
    /// </summary>
    [JsonIgnore]
    public bool HasChangelog => Changelog.Length > 0;

    /// <summary>
    /// List of qualified package names this package depends on
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
    public required string Url { get; init; }

    /// <summary>
    /// Optional URL to the package's Markdown help page
    /// </summary>
    /// <example>https://dc.ameko.moe/scripts/9volt/9volt.example1.md</example>
    public string? HelpUrl { get; init; }

    /// <summary>
    /// Name of repository (automatic)
    /// </summary>
    public string? Repository { get; set; }

    public virtual bool Equals(Package? other)
    {
        if (other is null)
            return false;
        if (ReferenceEquals(this, other))
            return true;

        return Type == other.Type
            && QualifiedName == other.QualifiedName
            && Author == other.Author
            && IsBetaChannel == other.IsBetaChannel;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine((int)Type, QualifiedName, Author, IsBetaChannel);
    }

    /// <summary>
    /// Generate a Markdown-formated changelog
    /// </summary>
    /// <returns>Markdown-formatted changelog</returns>
    public string GenerateChangelog()
    {
        if (Changelog.Length == 0)
            return string.Empty;

        StringBuilder sb = new();
        sb.AppendLine($"# {DisplayName}");

        foreach (var entry in Changelog.OrderByDescending(e => e.Version))
        {
            sb.AppendLine($"## {entry.Version}");
            sb.AppendLine();

            if (entry.Added?.Length > 0)
            {
                sb.AppendLine("### Additions");
                foreach (var addition in entry.Added)
                {
                    sb.AppendLine($"* {addition}");
                }
            }

            if (entry.Fixed?.Length > 0)
            {
                sb.AppendLine("### Fixes");
                foreach (var fix in entry.Fixed)
                {
                    sb.AppendLine($"* {fix}");
                }
            }

            if (entry.Changed?.Length > 0)
            {
                sb.AppendLine("### Changes");
                foreach (var change in entry.Changed)
                {
                    sb.AppendLine($"* {change}");
                }
            }

            if (entry.Removed?.Length > 0)
            {
                sb.AppendLine("### Removals");
                foreach (var removal in entry.Removed)
                {
                    sb.AppendLine($"* {removal}");
                }
            }

            if (entry.Deprecated?.Length > 0)
            {
                sb.AppendLine("### Deprecations");
                foreach (var change in entry.Deprecated)
                {
                    sb.AppendLine($"* {change}");
                }
            }
        }
        return sb.ToString();
    }

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

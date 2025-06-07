// SPDX-License-Identifier: MPL-2.0

namespace Holo.Scripting.Models;

/// <summary>
/// Information about a <see cref="HoloScript"/> or <see cref="HoloLibrary"/>
/// </summary>
public class ModuleInfo
{
    /// <summary>
    /// Name of the script
    /// </summary>
    public required string DisplayName { get; init; }

    /// <summary>
    /// Uniquely identifying name
    /// </summary>
    /// <remarks>
    /// The most common format is <c>author.scriptName</c>, but this isn't a requirement.
    /// The script's filename should match the qualified name -
    /// either <c>author.scriptName.cs</c> for <see cref="HoloScript"/>s
    /// or <c>author.libraryName.lib.cs</c> for libraries
    /// </remarks>
    public required string QualifiedName { get; init; }

    /// <summary>
    /// A short description of the script
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// Name of the script author(s)
    /// </summary>
    public required string Author { get; init; }

    /// <summary>
    /// Major.Minor script version. Used for Dependency Control updates
    /// </summary>
    public required decimal Version { get; init; }

    /// <summary>
    /// A list of qualified method names for export
    /// </summary>
    /// <remarks>
    /// <para>Method qualified names must follow the format <c>[scriptQName].funcName</c>. The funcName must not contain full stops.</para>
    /// <para>The method qualified name (after the last full stop) will be passed to <see cref="HoloScript.ExecuteAsync(string)"/></para>
    /// </remarks>
    public required string[] Exports { get; init; }

    /// <summary>
    /// Defines the behavior of the log window
    /// </summary>
    public required LogDisplay LogDisplay { get; init; }

    /// <summary>
    /// Optional sub-menu name for categorization
    /// </summary>
    public string? Submenu { get; init; }
}

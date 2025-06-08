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
    /// A list of methods for export
    /// </summary>
    /// <remarks>Only applies to <see cref="HoloScript"/>s. Will be ignored otherwise.</remarks>
    public MethodInfo[] Exports { get; init; } = [];

    /// <summary>
    /// Defines the behavior of the log window
    /// </summary>
    /// <remarks>Only applies to <see cref="HoloScript"/>s. Will be ignored otherwise.</remarks>
    public LogDisplay LogDisplay { get; init; } = LogDisplay.OnError;

    /// <summary>
    /// Optional sub-menu name for categorization
    /// </summary>
    /// <remarks>Only applies to <see cref="HoloScript"/>s. Will be ignored otherwise.</remarks>
    public string? Submenu { get; init; }
}

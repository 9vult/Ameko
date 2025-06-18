// SPDX-License-Identifier: MPL-2.0

namespace Holo.Scripting.Models;

/// <summary>
/// Basic information about a <see cref="HoloScript"/> or <see cref="HoloLibrary"/>
/// </summary>
/// <remarks>
/// This contains different information than <see cref="Module"/> does.
/// <para>
/// <see cref="Module"/> contains display information (<see cref="Module.Description"/>, etc.)
/// as well as <see cref="DependencyControl"/> data (<see cref="Module.Version"/>, etc.)
/// </para><para>
/// Meanwhile, <see cref="ModuleInfo"/> contains run-time information,
/// like which <see cref="Submenu"/> to use.
/// </para>
/// </remarks>
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

    /// <summary>
    /// Exclude the main entry point from the scripts menu
    /// </summary>
    /// <remarks>This flag will be ignored if there are no <see cref="Exports"/>.</remarks>
    public bool Headless { get; init; } = false;
}

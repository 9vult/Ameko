// SPDX-License-Identifier: MPL-2.0

namespace Holo.Models;

/// <summary>
/// A simple link between a <see cref="Workspace"/> and a <see cref="Uri"/>
/// </summary>
/// <param name="id">Workspace ID</param>
/// <param name="workspace">The linked workspace</param>
/// <param name="uri">The linked URI</param>
public struct Link(int id, Workspace? workspace = null, Uri? uri = null)
{
    /// <summary>
    /// ID of the Link
    /// </summary>
    public int Id { get; } = id;

    /// <summary>
    /// Linked workspace
    /// </summary>
    public Workspace? Workspace { get; set; } = workspace;

    /// <summary>
    /// Save location URI
    /// </summary>
    public Uri? Uri { get; set; } = uri;

    /// <summary>
    /// Indicates whether the <see cref="Workspace"/>
    /// is (or can be) read from disk
    /// </summary>
    public readonly bool IsSaved => Uri is not null;

    /// <summary>
    /// Indicated whether the <see cref="Workspace"/>
    /// is currently loaded in the <see cref="Solution"/>
    /// </summary>
    public readonly bool IsLoaded => Workspace is not null;

    /// <summary>
    /// Title of the link
    /// </summary>
    public readonly string Title =>
        Workspace is not null ? Workspace.DisplayTitle
        : Uri is not null ? Path.GetFileNameWithoutExtension(Uri.LocalPath)
        : "Untitled";
}

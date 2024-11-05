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
    public int Id = id;
    public Workspace? Workspace = workspace;
    public Uri? Uri = uri;

    /// <summary>
    /// Indicates whether the <see cref="Workspace"/>
    /// is (or can be) read from disk
    /// </summary>
    public readonly bool IsSaved => Uri != null;

    /// <summary>
    /// Indicated whether the <see cref="Workspace"/>
    /// is currently loaded in the <see cref="Solution"/>
    /// </summary>
    public readonly bool IsLoaded => Workspace != null;
}

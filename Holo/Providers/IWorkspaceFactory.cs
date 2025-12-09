// SPDX-License-Identifier: MPL-2.0

using AssCS;

namespace Holo.Providers;

public interface IWorkspaceFactory
{
    /// <summary>
    /// Create a <see cref="Workspace"/>
    /// </summary>
    /// <param name="document">Document in the workspace</param>
    /// <param name="id">Unique ID</param>
    /// <param name="savePath"><paramref name="document"/>'s save path, if applicable</param>
    /// <returns>The created workspace</returns>
    Workspace Create(Document document, int id, Uri? savePath = null);
}

// SPDX-License-Identifier: MPL-2.0

using AssCS;
using Holo;
using Holo.Providers;
using Microsoft.Extensions.Logging.Abstractions;

namespace TestingUtils;

public class TestWorkspaceFactory : IWorkspaceFactory
{
    /// <inheritdoc />
    public Workspace Create(Document document, int id, Uri? savePath = null)
    {
        return new Workspace(
            document,
            id,
            savePath,
            NullLogger<Workspace>.Instance,
            new MediaController(new NullSourceProvider(), NullLogger<MediaController>.Instance)
        );
    }
}

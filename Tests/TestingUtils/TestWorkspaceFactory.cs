// SPDX-License-Identifier: MPL-2.0

using System.IO.Abstractions.TestingHelpers;
using AssCS;
using Holo;
using Holo.Configuration;
using Holo.Providers;
using Microsoft.Extensions.Logging.Abstractions;

namespace TestingUtils;

public class TestWorkspaceFactory : IWorkspaceFactory
{
    /// <inheritdoc />
    public Workspace Create(Document document, int id, Uri? savePath = null)
    {
        var persist = new Persistence(new MockFileSystem(), NullLogger<Persistence>.Instance);
        var config = new Configuration(new MockFileSystem(), NullLogger<Configuration>.Instance);

        return new Workspace(
            document,
            id,
            savePath,
            NullLogger<Workspace>.Instance,
            config,
            new MediaController(
                new NullSourceProvider(),
                NullLogger<MediaController>.Instance,
                persist
            )
        );
    }
}

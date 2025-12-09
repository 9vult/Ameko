// SPDX-License-Identifier: MPL-2.0

using AssCS;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Holo.Providers;

public class WorkspaceFactory(IServiceProvider provider, ILoggerFactory loggerFactory)
    : IWorkspaceFactory
{
    /// <inheritdoc />
    public Workspace Create(Document document, int id, Uri? savePath = null)
    {
        return new Workspace(
            document,
            id,
            savePath,
            loggerFactory.CreateLogger<Workspace>(),
            provider.GetRequiredService<MediaController>()
        );
    }
}

// SPDX-License-Identifier: MPL-2.0

using System.IO.Abstractions;
using AssCS;
using Microsoft.Extensions.Logging;

namespace Holo.Providers;

public class ProjectProvider(IFileSystem fileSystem, ILoggerFactory loggerFactory)
    : BindableBase,
        IProjectProvider
{
    private Project _current = new(fileSystem, loggerFactory.CreateLogger<Project>());

    /// <inheritdoc />
    public Project Current
    {
        get => _current;
        set => SetProperty(ref _current, value);
    }

    /// <inheritdoc />
    public Project Create(bool isEmpty = false)
    {
        var logger = loggerFactory.CreateLogger<Project>();
        return new Project(fileSystem, logger, isEmpty);
    }

    /// <inheritdoc />
    public Project CreateFromFile(Uri uri)
    {
        var logger = loggerFactory.CreateLogger<Project>();
        return new Project(fileSystem, logger, uri);
    }

    public Project CreateFromDirectory(Uri uri)
    {
        var logger = loggerFactory.CreateLogger<Project>();
        return new Project(fileSystem, logger, uri);
    }
}

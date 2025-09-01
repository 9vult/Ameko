// SPDX-License-Identifier: MPL-2.0

using System.IO.Abstractions;
using AssCS;

namespace Holo.Providers;

public class ProjectProvider(IFileSystem fileSystem) : BindableBase, IProjectProvider
{
    private Project _current = new(fileSystem);

    public Project Current
    {
        get => _current;
        set => SetProperty(ref _current, value);
    }
}

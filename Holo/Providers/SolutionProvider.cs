// SPDX-License-Identifier: MPL-2.0

using System.IO.Abstractions;
using AssCS;

namespace Holo.Providers;

public class SolutionProvider(IFileSystem fileSystem) : BindableBase, ISolutionProvider
{
    private Solution _current = new(fileSystem);

    public Solution Current
    {
        get => _current;
        set => SetProperty(ref _current, value);
    }
}

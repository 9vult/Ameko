// SPDX-License-Identifier: MPL-2.0

using AssCS;

namespace Holo.Providers;

public class SolutionProvider : BindableBase, ISolutionProvider
{
    private Solution _current = new();

    public Solution Current
    {
        get => _current;
        set => SetProperty(ref _current, value);
    }
}

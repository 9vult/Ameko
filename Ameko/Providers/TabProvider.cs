// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.Collections.Generic;
using Ameko.ViewModels.Controls;
using Holo;
using Microsoft.Extensions.DependencyInjection;

namespace Ameko.Providers;

public class TabProvider(IServiceProvider provider)
{
    private readonly Dictionary<Workspace, TabItemViewModel> _cache = [];

    public TabItemViewModel Create(Workspace workspace)
    {
        if (_cache.TryGetValue(workspace, out var existing))
            return existing;

        var vm = ActivatorUtilities.CreateInstance<TabItemViewModel>(provider, workspace);
        _cache[workspace] = vm;
        return vm;
    }

    public void Release(Workspace workspace)
    {
        _cache.Remove(workspace);
    }
}

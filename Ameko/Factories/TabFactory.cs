// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.Collections.Generic;
using Ameko.ViewModels.Controls;
using Holo;
using Microsoft.Extensions.DependencyInjection;

namespace Ameko.Factories;

public interface ITabFactory
{
    /// <summary>
    /// Create a <see cref="TabItemViewModel"/> for the given <see cref="Workspace"/>
    /// </summary>
    /// <param name="workspace">Workspace needing a tab</param>
    /// <returns>ViewModel for the tab</returns>
    /// <remarks>ViewModels are cached. When closing the tab, <see cref="Release"/> must be called!</remarks>
    TabItemViewModel Create(Workspace workspace);

    /// <summary>
    /// Remove the <see cref="Workspace"/>'s <see cref="TabItemViewModel"/> from the cache
    /// </summary>
    /// <param name="workspace"></param>
    void Release(Workspace workspace);
}

public class TabFactory(IServiceProvider provider) : ITabFactory
{
    private readonly Dictionary<Workspace, TabItemViewModel> _cache = [];

    /// <inheritdoc cref="ITabFactory.Create"/>
    public TabItemViewModel Create(Workspace workspace)
    {
        if (_cache.TryGetValue(workspace, out var existing))
            return existing;

        var vm = ActivatorUtilities.CreateInstance<TabItemViewModel>(provider, workspace);
        _cache[workspace] = vm;
        return vm;
    }

    /// <inheritdoc cref="ITabFactory.Release"/>
    public void Release(Workspace workspace)
    {
        _cache.Remove(workspace);
    }
}

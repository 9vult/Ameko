// SPDX-License-Identifier: GPL-3.0-only

using System.Collections.Generic;
using Ameko.ViewModels.Controls;
using Holo;

namespace Ameko.Services;

/// <summary>
/// Service for managing <see cref="TabItemViewModel"/>s for <see cref="Workspace"/>s
/// </summary>
public static class WorkspaceViewModelService
{
    private static readonly Dictionary<int, TabItemViewModel> Registry = new();

    /// <summary>
    /// Register a new ViewModel
    /// </summary>
    /// <param name="id">ID of the <see cref="Workspace"/></param>
    /// <param name="vm"><see cref="TabItemViewModel"/> to add</param>
    public static void Register(int id, TabItemViewModel vm)
    {
        Registry.Add(id, vm);
    }

    /// <summary>
    /// Deregister a ViewModel
    /// </summary>
    /// <param name="id">ID of the <see cref="Workspace"/></param>
    /// <returns><see langword="true"/> if removed</returns>
    public static bool Deregister(int id)
    {
        return Registry.Remove(id);
    }

    /// <summary>
    /// Get a ViewModel by ID
    /// </summary>
    /// <param name="id">ID of the <see cref="Workspace"/></param>
    /// <param name="vm">Potential <see cref="TabItemViewModel"/></param>
    /// <returns><see langword="true"/> if successful</returns>
    public static bool TryGetValue(int id, out TabItemViewModel? vm)
    {
        return Registry.TryGetValue(id, out vm);
    }

    /// <summary>
    /// Clear the registry
    /// </summary>
    public static void Clear()
    {
        Registry.Clear();
    }
}

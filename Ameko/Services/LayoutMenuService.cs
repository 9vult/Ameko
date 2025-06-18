// SPDX-License-Identifier: GPL-3.0-only

using System.Collections.Generic;
using System.Windows.Input;
using Avalonia.Controls;
using Holo.Models;
using Material.Icons;
using Material.Icons.Avalonia;

namespace Ameko.Services;

/// <summary>
/// Service for managing the Layouts menu
/// </summary>
public class LayoutMenuService
{
    public static List<MenuItem> GenerateMenuItemSource(
        IList<TabLayout> layouts,
        ICommand selectLayoutCommand
    )
    {
        var congregation = new List<MenuItem>();

        foreach (var layout in layouts)
        {
            if (string.IsNullOrWhiteSpace(layout.Name))
                continue;

            congregation.Add(
                new MenuItem
                {
                    Header = layout.Name,
                    Command = selectLayoutCommand,
                    CommandParameter = layout.Name,
                    Icon = new MaterialIcon { Kind = MaterialIconKind.Artboard },
                }
            );
        }
        return congregation;
    }

    public static MenuItem GenerateReloadMenuItem(ICommand reloadCommand)
    {
        return new MenuItem
        {
            Header = I18N.Resources.Menu_ReloadLayouts,
            Command = reloadCommand,
            Icon = new MaterialIcon { Kind = MaterialIconKind.Reload },
        };
    }
}

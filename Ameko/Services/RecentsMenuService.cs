// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Input;
using Avalonia.Controls;
using Material.Icons;
using Material.Icons.Avalonia;

namespace Ameko.Services;

/// <summary>
/// Service for managing the Recents menus
/// </summary>
public static class RecentsMenuService
{
    public static List<MenuItem> GenerateMenuItemSource(
        IReadOnlyList<Uri> uris,
        ICommand openCommand
    )
    {
        var congregation = new List<MenuItem>();

        foreach (var uri in uris)
        {
            var path = uri.LocalPath;

            if (string.IsNullOrWhiteSpace(path))
                continue;

            congregation.Add(
                new MenuItem
                {
                    Header = Path.GetFileName(path),
                    Command = openCommand,
                    CommandParameter = uri,
                }
            );
        }
        return congregation;
    }

    public static List<NativeMenuItem> GenerateNativeMenuItemSource(
        IReadOnlyList<Uri> uris,
        ICommand openCommand
    )
    {
        var congregation = new List<NativeMenuItem>();

        foreach (var uri in uris)
        {
            var path = uri.LocalPath;

            if (string.IsNullOrWhiteSpace(path))
                continue;

            congregation.Add(
                new NativeMenuItem
                {
                    Header = Path.GetFileName(path),
                    Command = openCommand,
                    CommandParameter = uri,
                }
            );
        }
        return congregation;
    }

    public static MenuItem GenerateClearMenuItem(ICommand clearCommand)
    {
        return new MenuItem
        {
            Header = I18N.Resources.Menu_RecentClear,
            Command = clearCommand,
            Icon = new MaterialIcon { Kind = MaterialIconKind.Reload },
        };
    }

    public static NativeMenuItem GenerateClearNativeMenuItem(ICommand clearCommand)
    {
        return new NativeMenuItem
        {
            Header = I18N.Resources.Menu_RecentClear,
            Command = clearCommand,
        };
    }
}

// SPDX-License-Identifier: GPL-3.0-only

using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Avalonia.Controls;
using DynamicData;
using Holo.Scripting;
using Material.Icons;
using Material.Icons.Avalonia;

namespace Ameko.Services;

/// <summary>
/// Service for managing the Scripts menu
/// </summary>
public static class ScriptMenuService
{
    public static List<MenuItem> GenerateMenuItemSource(
        IList<IHoloExecutable> scripts,
        ICommand executeScriptCommand
    )
    {
        var congregation = new List<MenuItem>();

        var subItemsMap = new Dictionary<string, List<MenuItem>>();
        var rootItems = new List<MenuItem>();

        foreach (var script in scripts)
        {
            var menu = new MenuItem
            {
                Header = script.Info.DisplayName,
                Command = executeScriptCommand,
                CommandParameter = script.Info.QualifiedName,
                Icon = new MaterialIcon { Kind = MaterialIconKind.CodeBlockBraces },
            };

            var headless = script.Info.Headless && script.Info.Exports.Length != 0;

            if (!headless) // Only add if not headless
            {
                if (script.Info.Submenu is not null)
                {
                    if (!subItemsMap.ContainsKey(script.Info.Submenu))
                        subItemsMap[script.Info.Submenu] = [];
                    subItemsMap[script.Info.Submenu].Add(menu);
                }
                else
                {
                    rootItems.Add(menu);
                }
            }

            foreach (var methodInfo in script.Info.Exports)
            {
                var fullQualifiedName = $"{script.Info.QualifiedName}+{methodInfo.QualifiedName}";
                var methodMenu = new MenuItem
                {
                    Header = methodInfo.DisplayName,
                    Command = executeScriptCommand,
                    CommandParameter = fullQualifiedName,
                    Icon = new MaterialIcon { Kind = MaterialIconKind.CodeBlockParentheses },
                };

                if (methodInfo.Submenu is not null)
                {
                    if (!subItemsMap.ContainsKey(methodInfo.Submenu))
                        subItemsMap[methodInfo.Submenu] = [];
                    subItemsMap[methodInfo.Submenu].Add(methodMenu);
                }
                else
                {
                    rootItems.Add(methodMenu);
                }
            }
        }

        var groups = subItemsMap
            .Select(sub => new MenuItem { Header = sub.Key, ItemsSource = sub.Value })
            .ToList();

        congregation.AddRange(groups);
        congregation.AddRange(rootItems);
        return congregation;
    }

    public static List<NativeMenuItem> GenerateNativeMenuItemSource(
        IList<IHoloExecutable> scripts,
        ICommand executeScriptCommand
    )
    {
        var congregation = new List<NativeMenuItem>();

        var subItemsMap = new Dictionary<string, List<NativeMenuItem>>();
        var rootItems = new List<NativeMenuItem>();

        foreach (var script in scripts)
        {
            var menu = new NativeMenuItem
            {
                Header = script.Info.DisplayName,
                Command = executeScriptCommand,
                CommandParameter = script.Info.QualifiedName,
            };

            var headless = script.Info.Headless && script.Info.Exports.Length != 0;

            if (!headless) // Only add if not headless
            {
                if (script.Info.Submenu is not null)
                {
                    if (!subItemsMap.ContainsKey(script.Info.Submenu))
                        subItemsMap[script.Info.Submenu] = [];
                    subItemsMap[script.Info.Submenu].Add(menu);
                }
                else
                {
                    rootItems.Add(menu);
                }
            }

            foreach (var methodInfo in script.Info.Exports)
            {
                var fullQualifiedName = $"{script.Info.QualifiedName}+{methodInfo.QualifiedName}";
                var methodMenu = new NativeMenuItem
                {
                    Header = methodInfo.DisplayName,
                    Command = executeScriptCommand,
                    CommandParameter = fullQualifiedName,
                };

                if (methodInfo.Submenu is not null)
                {
                    if (!subItemsMap.ContainsKey(methodInfo.Submenu))
                        subItemsMap[methodInfo.Submenu] = [];
                    subItemsMap[methodInfo.Submenu].Add(methodMenu);
                }
                else
                {
                    rootItems.Add(methodMenu);
                }
            }
        }

        var groups = subItemsMap
            .Select(sub =>
            {
                var menu = new NativeMenu();
                menu.Items.AddRange(sub.Value);
                return new NativeMenuItem { Header = sub.Key, Menu = menu };
            })
            .ToList();

        congregation.AddRange(groups);
        congregation.AddRange(rootItems);
        return congregation;
    }

    public static MenuItem GenerateReloadMenuItem(ICommand reloadCommand)
    {
        return new MenuItem
        {
            Header = I18N.Resources.Menu_ReloadScripts,
            Command = reloadCommand,
            CommandParameter = true,
            Icon = new MaterialIcon { Kind = MaterialIconKind.Reload },
        };
    }

    public static NativeMenuItem GenerateReloadNativeMenuItem(ICommand reloadCommand)
    {
        return new NativeMenuItem
        {
            Header = I18N.Resources.Menu_ReloadScripts,
            Command = reloadCommand,
            CommandParameter = true,
        };
    }

    public static MenuItem GeneratePkgManMenuItem(ICommand pkgManCommand)
    {
        return new MenuItem
        {
            Header = I18N.Resources.Menu_PackageManager,
            Command = pkgManCommand,
            Icon = new MaterialIcon { Kind = MaterialIconKind.PackageVariant },
        };
    }

    public static NativeMenuItem GeneratePkgManNativeMenuItem(ICommand pkgManCommand)
    {
        return new NativeMenuItem
        {
            Header = I18N.Resources.Menu_PackageManager,
            Command = pkgManCommand,
        };
    }
}

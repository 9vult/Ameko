// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Input;
using Ameko.ViewModels;
using Holo.Configuration.Keybinds;

namespace Ameko.Services;

/// <summary>
/// Uses reflection to index key-bound commands
/// and register them with a <see cref="IKeybindRegistrar"/>
/// </summary>
public class KeybindScannerService
{
    public FrozenDictionary<string, ICommand> CommandMap { get; }

    /// <summary>
    /// Scan all ViewModels in the assembly
    /// </summary>
    /// <param name="assembly">Assembly to scan</param>
    /// <param name="commands">Map to index commands into</param>
    /// <returns></returns>
    private static List<Keybind> ScanAllViewModelsInAssembly(
        Assembly assembly,
        ref Dictionary<string, ICommand> commands
    )
    {
        var keybinds = new List<Keybind>();
        foreach (var type in assembly.GetTypes())
        {
            if (!typeof(ViewModelBase).IsAssignableFrom(type) || type.IsAbstract)
                continue;

            if (Activator.CreateInstance(type) is ViewModelBase vm)
            {
                keybinds.AddRange(ScanViewModel(vm, ref commands));
            }
        }
        return keybinds;
    }

    /// <summary>
    /// Scan a ViewModel
    /// </summary>
    /// <param name="viewModel">ViewModel to scan</param>
    /// <param name="commands">Map to index commands into</param>
    /// <returns></returns>
    private static List<Keybind> ScanViewModel(
        ViewModelBase viewModel,
        ref Dictionary<string, ICommand> commands
    )
    {
        var keybinds = new List<Keybind>();
        var type = viewModel.GetType();

        var classContext =
            type.GetCustomAttribute<KeybindContextAttribute>()?.DefaultContext
            ?? KeybindContext.None;

        foreach (
            var member in type.GetMembers(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
            )
        )
        {
            var attr = member.GetCustomAttribute<KeybindTargetAttribute>();
            if (attr is null)
                continue;

            var qualifiedName = attr.QualifiedName;
            var defaultKey = attr.DefaultKey;
            var defaultContext = attr.DefaultContext ?? classContext;

            var command = member switch
            {
                PropertyInfo prop => prop.GetValue(viewModel) as ICommand,
                MethodInfo m when typeof(ICommand).IsAssignableFrom(m.ReturnType) => m.Invoke(
                    viewModel,
                    null
                ) as ICommand,
                _ => null,
            };

            if (command is null)
                continue;

            commands[qualifiedName] = command;
            keybinds.Add(new Keybind(qualifiedName, defaultKey, defaultContext));
        }
        return keybinds;
    }

    /// <summary>
    /// Initialize the keybind scanner service
    /// </summary>
    /// <param name="registrar">Keybind Registrar to use</param>
    /// <remarks>Scanning will commence automatically upon initialization</remarks>
    public KeybindScannerService(IKeybindRegistrar registrar)
    {
        var commands = new Dictionary<string, ICommand>();

        // Discover the keybinds
        var keybinds = ScanAllViewModelsInAssembly(typeof(App).Assembly, ref commands);

        // Register the keybinds
        registrar.RegisterKeybinds(keybinds);

        // Index the commands
        CommandMap = commands.ToFrozenDictionary();
    }
}

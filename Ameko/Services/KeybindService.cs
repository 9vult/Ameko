// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Input;
using Ameko.ViewModels;
using Avalonia.Input;
using Holo.Configuration.Keybinds;
using NLog;

namespace Ameko.Services;

/// <summary>
/// Uses reflection to index key-bound commands
/// and register them with a <see cref="IKeybindRegistrar"/>
/// </summary>
public class KeybindService : IKeybindService
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    private readonly FrozenDictionary<string, (Type, string)> _commandMap;

    /// <inheritdoc />
    public IKeybindRegistrar KeybindRegistrar { get; }

    /// <inheritdoc />
    public void AttachKeybinds(
        ViewModelBase viewModel,
        IInputElement target,
        KeybindContext context
    )
    {
        var viewModelType = viewModel.GetType();
        target.KeyBindings.Clear();

        var applicableKeybinds = KeybindRegistrar.GetKeybinds(context).ToList();

        foreach (var (qualifiedName, (declaringType, memberName)) in _commandMap)
        {
            if (!declaringType.IsAssignableFrom(viewModelType))
                continue;

            // Get command from ViewModel instance

            if (
                declaringType
                    .GetProperty(
                        memberName,
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                    )
                    ?.GetValue(viewModel)
                is not ICommand command
            )
                continue;

            // Find keybind associated with this qualified name
            var keybind = applicableKeybinds.FirstOrDefault(k => k.QualifiedName == qualifiedName);

            if (keybind?.Key is null)
                continue;

            try
            {
                // Parse key string into Avalonia gesture
                var gesture = KeyGesture.Parse(keybind.Key);

                // Attach to the target element
                var keyBinding = new KeyBinding { Gesture = gesture, Command = command };

                target.KeyBindings.Add(keyBinding);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }
    }

    /// <inheritdoc />
    public void AttachScriptKeybinds(
        ICommand executeScriptCommand,
        KeybindContext context,
        IInputElement target
    )
    {
        var scriptBindings = KeybindRegistrar
            .GetKeybinds(context)
            .Where(kb => !kb.IsBuiltin && !string.IsNullOrWhiteSpace(kb.Key));

        foreach (var scriptBinding in scriptBindings)
        {
            try
            {
                // Parse key string into Avalonia gesture
                var gesture = KeyGesture.Parse(scriptBinding.Key!);

                // Attach to the target element
                var keyBinding = new KeyBinding
                {
                    Gesture = gesture,
                    Command = executeScriptCommand,
                    CommandParameter = scriptBinding.QualifiedName,
                };

                target.KeyBindings.Add(keyBinding);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }
    }

    /// <summary>
    /// Uses reflection to find all key-bound commands in the <paramref name="assembly"/>
    /// </summary>
    /// <param name="assembly">Assembly to scan</param>
    /// <returns>Keybind metadata</returns>
    private static IEnumerable<KeybindMetadata> ScanAllViewModelsInAssembly(Assembly assembly)
    {
        // ReSharper disable once InconsistentNaming
        var isMacOS = OperatingSystem.IsMacOS();
        const string cmd = "Cmd";
        const string ctrl = "Ctrl";

        foreach (var type in assembly.GetTypes())
        {
            if (!typeof(ViewModelBase).IsAssignableFrom(type))
                continue;

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

                var context = attr.DefaultContext ?? classContext;

                // Replace Ctrl with Cmd if running on a Mac
                var defaultKey =
                    isMacOS && attr.DefaultKey is not null
                        ? attr.DefaultKey.Replace(ctrl, cmd)
                        : attr.DefaultKey;

                yield return new KeybindMetadata
                {
                    QualifiedName = attr.QualifiedName,
                    DefaultKey = defaultKey,
                    DefaultContext = context,
                    DeclaringType = type,
                    MemberName = member.Name,
                };
            }
        }
    }

    /// <summary>
    /// Initialize the keybind scanner service
    /// </summary>
    /// <param name="registrar">Keybind Registrar to use</param>
    /// <remarks>Initial scanning will commence automatically upon initialization</remarks>
    public KeybindService(IKeybindRegistrar registrar)
    {
        KeybindRegistrar = registrar;

        // Discover the keybinds
        Logger.Info("Searching for keybind targets...");
        var metadata = ScanAllViewModelsInAssembly(typeof(App).Assembly)
            .Where(m => m.DeclaringType is not null && m.MemberName is not null)
            .ToList();
        Logger.Info($"Found {metadata.Count} targets");

        // Register the keybinds
        KeybindRegistrar.RegisterKeybinds(
            metadata
                .Select(m => new Keybind(m.QualifiedName, m.DefaultKey, m.DefaultContext))
                .ToList(),
            false
        );

        // Load in user keybinds
        KeybindRegistrar.Parse();

        // Index the commands
        _commandMap = metadata.ToFrozenDictionary(
            m => m.QualifiedName,
            m => (m.DeclaringType!, m.MemberName!)
        );
    }

    private class KeybindMetadata
    {
        public required string QualifiedName { get; init; }
        public string? DefaultKey { get; init; }
        public KeybindContext DefaultContext { get; init; }
        public Type? DeclaringType { get; init; }
        public string? MemberName { get; init; }
    }
}

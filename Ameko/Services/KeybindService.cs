// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Input;
using Ameko.ViewModels;
using Ameko.ViewModels.Windows;
using Avalonia.Input;
using Holo.Configuration.Keybinds;
using Holo.Providers;
using Microsoft.Extensions.Logging;

namespace Ameko.Services;

/// <summary>
/// Uses reflection to index key-bound commands
/// and register them with a <see cref="IKeybindRegistrar"/>
/// </summary>
public class KeybindService : IKeybindService
{
    private readonly ILogger _logger;

    /// <inheritdoc />
    public IKeybindRegistrar KeybindRegistrar { get; }

    private readonly ICommandRegistrar _commandRegistrar;
    private readonly MainWindowViewModel _mainWindowViewModel;

    /// <inheritdoc />
    public void RegisterCommands(ViewModelBase viewModel, int contextId)
    {
        _commandRegistrar.CreateContext(contextId);

        var infos = ScanForCommands(viewModel)
            .Concat(ScanForCommands(_mainWindowViewModel))
            .ToList();
        foreach (var info in infos)
        {
            _commandRegistrar.RegisterCommand(contextId, info.QualifiedName, info.Command);
        }

        // Try registering these defaults
        var newBinds = KeybindRegistrar.RegisterKeybinds(
            infos
                .Select(m => new Keybind(m.QualifiedName, m.DefaultKey, m.DefaultContext))
                .ToList(),
            false
        );

        // If the defaults were accepted, load in the user's preferences
        if (!newBinds)
            return;

        _logger.LogDebug("Registered {Count} keybind targets", infos.Count);
        KeybindRegistrar.Parse();
    }

    /// <inheritdoc />
    public void AttachKeybinds(KeybindContext context, IInputElement target, int cmdContextId)
    {
        target.KeyBindings.Clear();

        var applicableKeybinds = KeybindRegistrar.GetKeybinds(context).ToList();
        var commands = _commandRegistrar.GetCommandsForContext(cmdContextId);

        foreach (var keybind in applicableKeybinds)
        {
            if (keybind.Key is null)
                continue;

            // Try to find the associated command
            var command = commands.GetValueOrDefault(keybind.QualifiedName);
            if (command is null)
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
                _logger.LogError(ex, "Failed to parse keybind");
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
                _logger.LogError(ex, "Failed to parse script keybind");
            }
        }
    }

    private static IEnumerable<CommandMetadata> ScanForCommands(ViewModelBase viewModel)
    {
        var type = viewModel.GetType();
        var props = type.GetProperties(
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
        );

        foreach (var prop in props)
        {
            if (!typeof(ICommand).IsAssignableFrom(prop.PropertyType))
                continue;

            var attrib = prop.GetCustomAttribute<KeybindTargetAttribute>();
            if (attrib is null)
                continue;

            if (prop.GetValue(viewModel) is not ICommand command)
                continue;

            yield return new CommandMetadata
            {
                Command = command,
                QualifiedName = attrib.QualifiedName,
                DefaultKey = attrib.DefaultKey,
                DefaultContext = attrib.DefaultContext,
            };
        }
    }

    /// <summary>
    /// Initialize the keybind scanner service
    /// </summary>
    /// <param name="keybindRegistrar">Keybind Registrar to use</param>
    /// <param name="commandRegistrar">Command Registrar to use</param>
    /// <param name="mainWindowViewModel">Main Window's ViewModel</param>
    /// <param name="logger">Logger to use</param>
    /// <remarks>Initial scanning will commence automatically upon initialization</remarks>
    public KeybindService(
        IKeybindRegistrar keybindRegistrar,
        ICommandRegistrar commandRegistrar,
        MainWindowViewModel mainWindowViewModel,
        ILogger<KeybindService> logger
    )
    {
        KeybindRegistrar = keybindRegistrar;
        _commandRegistrar = commandRegistrar;
        _mainWindowViewModel = mainWindowViewModel;
        _logger = logger;
    }

    private class CommandMetadata
    {
        public required ICommand Command { get; init; }
        public required string QualifiedName { get; init; }
        public required string? DefaultKey { get; init; }
        public required KeybindContext DefaultContext { get; init; }
    }
}

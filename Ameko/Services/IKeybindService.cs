// SPDX-License-Identifier: GPL-3.0-only

using System.Windows.Input;
using Ameko.ViewModels;
using Avalonia.Input;
using Holo.Configuration.Keybinds;

namespace Ameko.Services;

public interface IKeybindService
{
    /// <summary>
    /// Expose the Keybind Registrar so VMs only have to pull in this service
    /// </summary>
    IKeybindRegistrar KeybindRegistrar { get; }

    /// <summary>
    /// Register commands to be used by keybinds
    /// </summary>
    /// <param name="viewModel">ViewModel containing the commands</param>
    /// <param name="contextId">Command context ID</param>
    void RegisterCommands(ViewModelBase viewModel, int contextId);

    /// <summary>
    /// Attach keybinds to a control
    /// </summary>
    /// <param name="context">Keybind context to attach</param>
    /// <param name="target">Control to attach the keybinds to</param>
    /// <param name="cmdContextId">Command context ID</param>
    void AttachKeybinds(KeybindContext context, IInputElement target, int cmdContextId);

    /// <summary>
    /// Register keybinds for script execution
    /// </summary>
    /// <param name="executeScriptCommand">Command handling script execution</param>
    /// <param name="context">Keybind context</param>
    /// <param name="target">Control to attach the keybinds to</param>
    /// <remarks>This method does not clear pre-existing keybinds</remarks>
    void AttachScriptKeybinds(
        ICommand executeScriptCommand,
        KeybindContext context,
        IInputElement target
    );
}

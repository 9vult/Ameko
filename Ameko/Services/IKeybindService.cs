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
    /// Attach keybinds to a ViewModel's commands
    /// </summary>
    /// <param name="viewModel">ViewModel to attach keybinds to</param>
    /// <param name="target">Window or Control handling the inputs</param>
    /// <remarks>This method clears pre-existing keybinds before adding new ones</remarks>
    void AttachKeybinds(ViewModelBase viewModel, IInputElement target);

    /// <summary>
    /// Register keybinds for script execution
    /// </summary>
    /// <param name="executeScriptCommand">Command handling script execution</param>
    /// <param name="context">Keybind context</param>
    /// <param name="target">Window or Control handling the input</param>
    /// <remarks>This method does not clear pre-existing keybinds</remarks>
    void AttachScriptKeybinds(
        ICommand executeScriptCommand,
        KeybindContext context,
        IInputElement target
    );
}

// SPDX-License-Identifier: MPL-2.0

using System.Windows.Input;

namespace Holo.Providers;

/// <summary>
/// Provides methods for registering and managing
/// commands and command contexts
/// </summary>
public interface ICommandRegistrar
{
    /// <summary>
    /// Create a context with the given <paramref name="id"/>
    /// </summary>
    /// <param name="id">Context identifier</param>
    /// <returns><see langword="true"/> if successful</returns>
    bool CreateContext(int id);

    /// <summary>
    /// Remove a context
    /// </summary>
    /// <param name="id">Context identifier</param>
    /// <returns><see langword="true"/> if successful</returns>
    bool DestroyContext(int id);

    /// <summary>
    /// Register a command
    /// </summary>
    /// <param name="contextId">Context identifier</param>
    /// <param name="qualifiedName">Uniquely-identifying name for the command</param>
    /// <param name="command">Instance of a command</param>
    /// <returns><see langword="true"/> if successful</returns>
    bool RegisterCommand(int contextId, string qualifiedName, ICommand command);

    /// <summary>
    /// Get the commands registered in a context
    /// </summary>
    /// <param name="contextId">Context identifier</param>
    /// <returns>Mapping between command identifiers and instances</returns>
    IReadOnlyDictionary<string, ICommand> GetCommandsForContext(int contextId);
}

// SPDX-License-Identifier: MPL-2.0

using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Holo.Providers;

public class CommandRegistrar : ICommandRegistrar
{
    private readonly Dictionary<int, Dictionary<string, ICommand>> _contexts = [];

    /// <inheritdoc />
    public bool CreateContext(int id)
    {
        return _contexts.TryAdd(id, new Dictionary<string, ICommand>());
    }

    /// <inheritdoc />
    public bool DestroyContext(int id)
    {
        return _contexts.Remove(id);
    }

    /// <inheritdoc />
    public bool RegisterCommand(int contextId, string qualifiedName, ICommand command)
    {
        return _contexts.TryGetValue(contextId, out var context)
            && context.TryAdd(qualifiedName, command);
    }

    /// <inheritdoc />
    public IReadOnlyDictionary<string, ICommand> GetCommandsForContext(int contextId)
    {
        return _contexts.TryGetValue(contextId, out var context)
            ? context.AsReadOnly()
            : ReadOnlyDictionary<string, ICommand>.Empty;
    }
}

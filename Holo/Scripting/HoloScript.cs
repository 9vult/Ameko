// SPDX-License-Identifier: MPL-2.0

using Holo.Providers;
using Holo.Scripting.Models;
using Microsoft.Extensions.Logging;

namespace Holo.Scripting;

/// <summary>
/// Base class for user scripts
/// </summary>
public abstract class HoloScript : IHoloExecutable
{
    /// <summary>
    /// Basic script information
    /// </summary>
    public ModuleInfo Info { get; }

    /// <summary>
    /// Script entry point
    /// </summary>
    /// <param name="methodName">
    /// Qualified name of the called Method, or <see langword="null"/> if no method is being called
    /// </param>
    /// <returns>Result of script execution</returns>
    /// <remarks>It is up to the implementer how to handle method execution</remarks>
    public abstract Task<ExecutionResult> ExecuteAsync(string? methodName);

    /// <summary>
    /// Initialize the script
    /// </summary>
    /// <param name="info">Script information</param>
    protected HoloScript(ModuleInfo info)
    {
        Info = info;
        Logger = ScriptServiceLocator.GetLogger(info.QualifiedName ?? GetType().Name);
    }

    // ReSharper disable once MemberCanBePrivate.Global
    protected ILogger Logger { get; }
}

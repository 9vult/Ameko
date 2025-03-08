// SPDX-License-Identifier: MPL-2.0

using NLog;

namespace Holo.Scripting;

/// <summary>
/// Base class for user scripts
/// </summary>
public abstract class HoloScript
{
    /// <summary>
    /// Basic script information
    /// </summary>
    public virtual ScriptInfo Info { get; init; }

    /// <summary>
    /// Script entry point
    /// </summary>
    /// <returns>Result of script execution</returns>
    public abstract Task<ExecutionResult> ExecuteAsync();

    /// <summary>
    /// Entry point for exported methods
    /// </summary>
    /// <param name="methodName">Method's qualified name</param>
    /// <returns>Result of script execution</returns>
    /// <remarks>
    /// <para><b>You MUST override this method if your script uses exported methods!</b></para>
    /// <para>The default implementation calls <see cref="ExecuteAsync"/>, ignoring the <paramref name="methodName"/>.</para>
    /// </remarks>
    public virtual async Task<ExecutionResult> ExecuteAsync(string methodName)
    {
        Logger.Info($"Ignoring method {methodName} and executing normally");
        return await ExecuteAsync();
    }

    // ReSharper disable once MemberCanBePrivate.Global
    protected static ILogger Logger { get; } = LogManager.GetCurrentClassLogger();
}

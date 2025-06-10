// SPDX-License-Identifier: GPL-3.0-only

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Holo.Scripting;

namespace Ameko.Services;

public interface IScriptService
{
    /// <summary>
    /// List of currently-loaded scripts
    /// </summary>
    AssCS.Utilities.ReadOnlyObservableCollection<HoloScript> Scripts { get; }

    /// <summary>
    /// Get a script by its qualified name
    /// </summary>
    /// <param name="qualifiedName">Name of the script</param>
    /// <param name="script">Script, if found</param>
    /// <returns><see langword="true"/> if found</returns>
    bool TryGetScript(string qualifiedName, [NotNullWhen(true)] out HoloScript? script);

    /// <summary>
    /// Asynchronously execute a script or method
    /// </summary>
    /// <param name="qualifiedName">Name of the script or method</param>
    /// <returns>Execution result of the script or method</returns>
    /// <remarks>
    /// This method attempts to execute a script first, then a method.
    /// This relies on method names being <c>scriptName.methodName</c>.
    /// </remarks>
    Task<ExecutionResult> ExecuteScriptAsync(string qualifiedName);

    /// <summary>
    /// Reload scripts
    /// </summary>
    /// <param name="isManual">Whether to show the message box</param>
    Task Reload(bool isManual);

    /// <summary>
    /// Event Handler for <see cref="ScriptService.Reload"/>
    /// </summary>
    public delegate void ReloadEventHandler(object sender, EventArgs e);

    event ReloadEventHandler? OnReload;
}

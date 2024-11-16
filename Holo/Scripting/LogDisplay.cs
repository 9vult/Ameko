// SPDX-License-Identifier: MPL-2.0

namespace Holo.Scripting;

/// <summary>
/// Specifies the behavior of the <see cref="HoloScript"/> log window
/// </summary>
public enum LogDisplay
{
    /// <summary>
    /// The log window is displayed temporarily during script execution,
    /// and will close automatically if the script finishes successfully
    /// </summary>
    Ephemeral,

    /// <summary>
    /// The log window is not displayed unless there is an error during execution
    /// </summary>
    OnError,

    /// <summary>
    /// The log window is displayed during script execution, and is not closed
    /// regardless of whether the script completes successfully or not
    /// </summary>
    Forced,
}

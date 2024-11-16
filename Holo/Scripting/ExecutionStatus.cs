// SPDX-License-Identifier: MPL-2.0

namespace Holo.Scripting;

/// <summary>
/// Denotes the result of the execution of a <see cref="HoloScript"/>
/// </summary>
public enum ExecutionStatus
{
    /// <summary>
    /// The script executed successfully, with no error or warnings
    /// </summary>
    Success,

    /// <summary>
    /// The script was unable to complete successfully
    /// </summary>
    Failure,

    /// <summary>
    /// The script completed successfully, but there are warnings
    /// </summary>
    Warning,
}

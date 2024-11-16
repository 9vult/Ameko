// SPDX-License-Identifier: MPL-2.0

namespace Holo.Scripting;

/// <summary>
/// Represents the result of a script's execution
/// </summary>
public readonly struct ExecutionResult
{
    /// <summary>
    /// The status of the script upon exit
    /// </summary>
    public required ExecutionStatus Status { get; init; }

    /// <summary>
    /// An optional message providing additional details about the script's execution result
    /// </summary>
    public string? Message { get; init; }
}

// SPDX-License-Identifier: MPL-2.0

namespace Holo.Configuration;

/// <summary>
/// Denotes which fields should have changes propagated
/// from the active event to other selected events
/// </summary>
public enum PropagateFields
{
    /// <summary>
    /// Propagate all fields
    /// </summary>
    All = 0,

    /// <summary>
    /// Propagate no fields
    /// </summary>
    None = 1,

    /// <summary>
    /// Propagate everything except the Text field
    /// </summary>
    NonText = 2,
}

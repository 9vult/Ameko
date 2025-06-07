// SPDX-License-Identifier: MPL-2.0

using System.Text.Json.Serialization;

namespace Holo.Models;

/// <summary>
/// Simplified representation of a <see cref="Globals"/> for
/// serialization purposes
/// </summary>
internal record GlobalsModel
{
    [JsonIgnore]
    internal const double CurrentApiVersion = 1.0d;

    public required double Version;
    public required string[] Styles;
    public required string[] Colors;
}

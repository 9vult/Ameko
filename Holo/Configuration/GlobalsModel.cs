// SPDX-License-Identifier: MPL-2.0

using System.Text.Json.Serialization;

namespace Holo.Configuration;

internal record GlobalsModelBase
{
    [JsonIgnore]
    internal const int CurrentApiVersion = 1;
    public required int Version;
}

/// <summary>
/// Simplified representation of a <see cref="Globals"/> for
/// serialization purposes
/// </summary>
internal record GlobalsModel : GlobalsModelBase
{
    public required string[] Styles;
    public required string[] Colors;
    public required string[] CustomWords;
}

#region Versions

internal record GlobalsModelV1 : GlobalsModelBase
{
    public required string[] Styles;
    public required string[] Colors;
    public required string[] CustomWords;
}

#endregion

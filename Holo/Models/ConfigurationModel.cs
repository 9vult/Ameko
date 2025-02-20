// SPDX-License-Identifier: MPL-2.0

using System.Text.Json.Serialization;

namespace Holo.Models;

internal record ConfigurationModel
{
    [JsonIgnore]
    internal const double CurrentApiVersion = 1.0d;

    public required double Version;
    public required int Cps;
    public required bool UseSoftLinebreaks;
    public required bool AutosaveEnabled;
    public required int AutosaveInterval;
}

// SPDX-License-Identifier: MPL-2.0

using System.Text.Json.Serialization;

namespace Holo.Configuration;

public class PersistenceModel
{
    [JsonIgnore]
    internal const double CurrentApiVersion = 1.0d;

    public required double Version;
    public required string LayoutName;
    public required bool UseColorRing;
    public required double VisualizationScaleX;
    public required double VisualizationScaleY;
    public required string PlaygroundCs;
    public required string PlaygroundJs;
}

// SPDX-License-Identifier: MPL-2.0

using System.Text.Json.Serialization;

namespace Holo.Models;

internal record ConfigurationModel
{
    [JsonIgnore]
    internal const double CurrentApiVersion = 1.0d;

    public required double Version;
    public required int Cps;
    public required bool CpsIncludesWhitespace;
    public required bool CpsIncludesPunctuation;
    public required bool UseSoftLinebreaks;
    public required bool AutosaveEnabled;
    public required int AutosaveInterval;
    public required bool LineWidthIncludesWhitespace;
    public required bool LineWidthIncludesPunctuation;
    public required string Culture;
    public required Theme Theme;
    public required string[] RepositoryUrls;
}

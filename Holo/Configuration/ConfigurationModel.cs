// SPDX-License-Identifier: MPL-2.0

using System.Text.Json.Serialization;
using Holo.Models;

namespace Holo.Configuration;

internal record ConfigurationModel
{
    [JsonIgnore]
    internal const double CurrentApiVersion = 1.0d;

    public required double Version;
    public required uint Cps;
    public required bool CpsIncludesWhitespace;
    public required bool CpsIncludesPunctuation;
    public required bool UseSoftLinebreaks;
    public required bool AutosaveEnabled;
    public required uint AutosaveInterval;
    public required bool LineWidthIncludesWhitespace;
    public required bool LineWidthIncludesPunctuation;
    public required bool DiscordRpcEnabled;
    public required int DefaultLayer;
    public required string Culture;
    public required string SpellcheckCulture;
    public required Theme Theme;
    public required uint GridPadding;
    public required PropagateFields PropagateFields;
    public required string[] RepositoryUrls;
}

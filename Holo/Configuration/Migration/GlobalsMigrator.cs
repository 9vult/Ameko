// SPDX-License-Identifier: MPL-2.0

using System.Text.Json;

namespace Holo.Configuration.Migration;

internal static class GlobalsMigrator
{
    private static readonly JsonSerializerOptions JsonOptions = new() { IncludeFields = true };
    private static readonly GlobalsMigratorCurrent CurrentMigrator = new();

    public static GlobalsModel? MigrateToCurrent(string content)
    {
        using var json = JsonDocument.Parse(content);

        if (!json.RootElement.TryGetProperty("Version", out var versionProp))
            return null;
        var version = versionProp.GetInt32();

        switch (version)
        {
            case 1:
                var v1 =
                    JsonSerializer.Deserialize<GlobalsModelV1>(content, JsonOptions)
                    ?? throw new InvalidDataException("Globals model deserialization failed");
                return CurrentMigrator.Migrate(v1);
        }
        return null;
    }
}

internal interface IGlobalsMigrator<in TIn, out TOut>
    where TIn : GlobalsModelBase
    where TOut : GlobalsModelBase
{
    /// <summary>
    /// Migrate a <paramref name="globals"/> with version
    /// <typeparamref name="TIn"/> to version <typeparamref name="TOut"/>
    /// </summary>
    /// <param name="globals">Globals to migrate</param>
    /// <returns>Migrated globals</returns>
    TOut Migrate(TIn globals);
}

/// <summary>
/// Migrate from the versioned model matching the current live model to the live model
/// </summary>
internal class GlobalsMigratorCurrent : IGlobalsMigrator<GlobalsModelV1, GlobalsModel>
{
    /// <inheritdoc />
    public GlobalsModel Migrate(GlobalsModelV1 globals)
    {
        return new GlobalsModel
        {
            Version = GlobalsModelBase.CurrentApiVersion,
            Styles = globals.Styles,
            Colors = globals.Colors,
            CustomWords = globals.CustomWords,
        };
    }
}

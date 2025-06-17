// SPDX-License-Identifier: MPL-2.0

using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions;
using System.Text.Json;
using System.Text.Json.Serialization;
using Holo.IO;
using NLog;

namespace Holo.Scripting;

public class ScriptConfigurationService(IFileSystem fileSystem) : IScriptConfigurationService
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        IncludeFields = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
    };
    private static readonly Uri ModulesRoot = new(
        Path.Combine(DirectoryService.DataHome, "scripts")
    );

    private readonly Dictionary<string, Dictionary<string, JsonElement>> _cache = [];

    /// <inheritdoc />
    public bool TryGet<T>(IHoloExecutable caller, string key, [NotNullWhen(true)] out T? value)
    {
        var qName = caller.Info.QualifiedName;
        if (!_cache.TryGetValue(qName, out var data))
            data ??= Read(qName);

        if (data.TryGetValue(key, out var element))
        {
            try
            {
                value = element.Deserialize<T>();
                return value is not null;
            }
            catch
            {
                value = default;
                return false;
            }
        }

        value = default;
        return false;
    }

    /// <inheritdoc />
    public void Set<T>(IHoloExecutable caller, string key, T value)
    {
        var qName = caller.Info.QualifiedName;
        if (!_cache.TryGetValue(qName, out var data))
            _cache[qName] = data ??= Read(qName);

        var element = JsonSerializer.SerializeToElement(value, JsonOptions);

        data[key] = element;

        Write(qName);
    }

    /// <inheritdoc />
    public bool Remove(IHoloExecutable caller, string key)
    {
        var qName = caller.Info.QualifiedName;
        if (!_cache.TryGetValue(qName, out var data))
            _cache[qName] = data ??= Read(qName);

        var result = data.Remove(key);
        if (result)
            Write(qName);
        return result;
    }

    /// <inheritdoc />
    public bool Contains(IHoloExecutable caller, string key)
    {
        var qName = caller.Info.QualifiedName;
        if (!_cache.TryGetValue(qName, out var data))
            _cache[qName] = data ??= Read(qName);
        return data.ContainsKey(key);
    }

    /// <summary>
    /// Read a module's configuration from disk
    /// </summary>
    /// <param name="qualifiedName">Module's qualified name</param>
    /// <returns>Configuration data if found, else empty</returns>
    private Dictionary<string, JsonElement> Read(string qualifiedName)
    {
        Logger.Trace($"Reading configuration file for module {qualifiedName}");
        var path = ConfigPath(qualifiedName);
        try
        {
            if (!fileSystem.Directory.Exists(Path.GetDirectoryName(path)))
                fileSystem.Directory.CreateDirectory(Path.GetDirectoryName(path) ?? "/");

            if (!fileSystem.File.Exists(path))
            {
                Logger.Trace("Configuration file does not exist, using empty...");
                return [];
            }

            using var fs = fileSystem.FileStream.New(
                path,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite
            );
            using var reader = new StreamReader(fs);

            var result = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(
                reader.ReadToEnd(),
                JsonOptions
            );
            Logger.Trace("Done!");
            return result ?? [];
        }
        catch (Exception ex) when (ex is IOException or JsonException)
        {
            Logger.Error(ex);
            Logger.Trace("Failed to parse configuration, using empty...");
            return [];
        }
    }

    /// <summary>
    /// Write a module's configuration data to disk
    /// </summary>
    /// <param name="qualifiedName">Module's qualified name</param>
    /// <returns>
    /// <see langword="true"/> if successful or skipped, <see langword="false"/> if failed
    /// </returns>
    private bool Write(string qualifiedName)
    {
        Logger.Trace($"Writing configuration file for module {qualifiedName}");
        var path = ConfigPath(qualifiedName);
        try
        {
            if (!fileSystem.Directory.Exists(Path.GetDirectoryName(path)))
                fileSystem.Directory.CreateDirectory(Path.GetDirectoryName(path) ?? "/");

            if (!_cache.TryGetValue(qualifiedName, out var data))
            {
                Logger.Trace($"No data found for module {qualifiedName}, skipping...");
                return true;
            }

            using var fs = fileSystem.FileStream.New(
                path,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None
            );
            using var writer = new StreamWriter(fs);

            var content = JsonSerializer.Serialize(data, JsonOptions);
            writer.Write(content);
            Logger.Trace("Done!");
            return true;
        }
        catch (Exception ex) when (ex is IOException or JsonException)
        {
            Logger.Error(ex);
            Logger.Trace("Failed to parse configuration, using empty...");
            return false;
        }
    }

    /// <summary>
    /// Get the filepath for a module
    /// </summary>
    /// <param name="qualifiedName">Module's qualified name</param>
    /// <returns>The filepath, ending in <c>.json</c></returns>
    public static string ConfigPath(string qualifiedName)
    {
        return Path.Combine(ModulesRoot.LocalPath, "configuration", $"{qualifiedName}.json");
    }
}

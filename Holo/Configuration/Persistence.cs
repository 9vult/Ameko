// SPDX-License-Identifier: MPL-2.0

using System.IO.Abstractions;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using AssCS;
using AssCS.Utilities;
using Holo.IO;
using Microsoft.Extensions.Logging;

namespace Holo.Configuration;

/// <summary>
/// <see cref="Configuration"/>-like utility for application persistence
/// not directly controlled by a user
/// </summary>
public class Persistence : BindableBase, IPersistence
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        IncludeFields = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
    };

    /// <summary>
    /// The filesystem being used
    /// </summary>
    private readonly IFileSystem _fileSystem;
    private readonly ILogger _logger;

    private string _layoutName;
    private bool _useColorRing;
    private string _playgroundCs;
    private string _playgroundJs;

    /// <inheritdoc />
    public string LayoutName
    {
        get => _layoutName;
        set => SetProperty(ref _layoutName, value);
    }

    /// <inheritdoc />
    public bool UseColorRing
    {
        get => _useColorRing;
        set => SetProperty(ref _useColorRing, value);
    }

    /// <inheritdoc />
    public string PlaygroundCs
    {
        get => _playgroundCs;
        set => SetProperty(ref _playgroundCs, value);
    }

    /// <inheritdoc />
    public string PlaygroundJs
    {
        get => _playgroundJs;
        set => SetProperty(ref _playgroundJs, value);
    }

    /// <inheritdoc />
    public bool Save()
    {
        var path = Paths.Persistence.LocalPath;
        _logger.LogInformation("Writing persistence to {Path}...", path);
        try
        {
            if (!_fileSystem.Directory.Exists(Path.GetDirectoryName(path)))
                _fileSystem.Directory.CreateDirectory(Path.GetDirectoryName(path) ?? "/");

            using var fs = _fileSystem.FileStream.New(
                path,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None
            );
            using var writer = new StreamWriter(fs);

            var model = new PersistenceModel
            {
                Version = PersistenceModel.CurrentApiVersion,
                LayoutName = _layoutName,
                UseColorRing = _useColorRing,
                PlaygroundCs = StringEncoder.Base64Encode(_playgroundCs),
                PlaygroundJs = StringEncoder.Base64Encode(_playgroundJs),
            };

            var content = JsonSerializer.Serialize(model, JsonOptions);
            writer.Write(content);
            _logger.LogInformation("Done!");
            return true;
        }
        catch (Exception ex) when (ex is IOException or JsonException)
        {
            _logger.LogError(ex, "Failed to write persistence");
            return false;
        }
    }

    public static Persistence Parse(IFileSystem fileSystem, ILogger<Persistence> logger)
    {
        logger.LogInformation("Parsing persistence...");
        var path = Paths.Persistence.LocalPath;
        try
        {
            if (!fileSystem.Directory.Exists(Path.GetDirectoryName(path)))
                fileSystem.Directory.CreateDirectory(Path.GetDirectoryName(path) ?? "/");

            if (!fileSystem.File.Exists(path))
            {
                logger.LogWarning("Persistence file does not exist, using defaults...");
                return new Persistence(fileSystem, logger);
            }

            using var fs = fileSystem.FileStream.New(
                path,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite
            );
            using var reader = new StreamReader(fs);

            var model =
                JsonSerializer.Deserialize<PersistenceModel>(reader.ReadToEnd(), JsonOptions)
                ?? throw new InvalidDataException("Persistence model deserialization failed!");

            var result = new Persistence(fileSystem, logger)
            {
                _layoutName = model.LayoutName,
                _useColorRing = model.UseColorRing,
                _playgroundCs = StringEncoder.Base64Decode(model.PlaygroundCs),
                _playgroundJs = StringEncoder.Base64Decode(model.PlaygroundJs),
            };
            logger.LogInformation("Done!");
            return result;
        }
        catch (Exception ex) when (ex is IOException or JsonException)
        {
            logger.LogError(ex, "Failed to parse persistence, using defaults...");
            return new Persistence(fileSystem, logger);
        }
    }

    /// <summary>
    /// Instantiate a Persistence instance
    /// </summary>
    /// <param name="fileSystem">FileSystem to use</param>
    /// <param name="logger">Logger to use</param>
    public Persistence(IFileSystem fileSystem, ILogger<Persistence> logger)
    {
        _fileSystem = fileSystem;
        _logger = logger;
        _layoutName = "Default";
        _useColorRing = false;
        _playgroundCs = string.Empty;
        _playgroundJs = string.Empty;
    }

    /// <inheritdoc />
    protected override bool SetProperty<T>(
        ref T storage,
        T value,
        [CallerMemberName] string? propertyName = null
    )
    {
        if (Equals(storage, value))
            return false;

        storage = value;
        RaisePropertyChanged(propertyName);
        return true;
    }
}

// SPDX-License-Identifier: MPL-2.0

using System.IO.Abstractions;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using AssCS;
using Holo.IO;
using NLog;

namespace Holo.Configuration;

/// <summary>
/// <see cref="Configuration"/>-like utility for application persistence
/// not directly controlled by a user
/// </summary>
public class Persistence : BindableBase, IPersistence
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        IncludeFields = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
    };

    /// <summary>
    /// The filesystem being used
    /// </summary>
    private readonly IFileSystem _fileSystem;

    private string _layoutName;
    private bool _useColorRing;

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

    public bool Save()
    {
        var path = Paths.Persistence.LocalPath;
        Logger.Info($"Writing persistence to {path}...");
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
            };

            var content = JsonSerializer.Serialize(model, JsonOptions);
            writer.Write(content);
            Logger.Info($"Done!");
            return true;
        }
        catch (Exception ex) when (ex is IOException or JsonException)
        {
            Logger.Error(ex);
            return false;
        }
    }

    public static Persistence Parse(IFileSystem fileSystem)
    {
        Logger.Info("Parsing persistence...");
        var path = Paths.Persistence.LocalPath;
        try
        {
            if (!fileSystem.Directory.Exists(Path.GetDirectoryName(path)))
                fileSystem.Directory.CreateDirectory(Path.GetDirectoryName(path) ?? "/");

            if (!fileSystem.File.Exists(path))
            {
                Logger.Info("Persistence file does not exist, using defaults...");
                return new Persistence(fileSystem);
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

            var result = new Persistence(fileSystem)
            {
                _layoutName = model.LayoutName,
                _useColorRing = model.UseColorRing,
            };
            Logger.Info("Done!");
            return result;
        }
        catch (Exception ex) when (ex is IOException or JsonException)
        {
            Logger.Error(ex);
            Logger.Info("Failed to parse persistence, using defaults...");
            return new Persistence(fileSystem);
        }
    }

    /// <summary>
    /// Instantiate a Persistence instance
    /// </summary>
    /// <param name="fileSystem">FileSystem to use</param>
    public Persistence(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
        _layoutName = "Default";
        _useColorRing = false;
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
        Save();
        return true;
    }
}

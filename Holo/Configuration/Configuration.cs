// SPDX-License-Identifier: MPL-2.0

using System.Collections.ObjectModel;
using System.IO.Abstractions;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using AssCS;
using Holo.IO;
using Holo.Models;
using NLog;

namespace Holo.Configuration;

/// <summary>
/// User-controlled configuration options.
/// </summary>
/// <remarks>
/// <para>
/// Some, but not all, options are configurable in both the application space
/// via <see cref="Configuration"/> files and in <see cref="Solution"/>s.
/// In such cases, the solution's value takes precedence, unless the
/// solution's value is <see langword="null"/>.
/// </para>
/// <para>
/// <see cref="Save()"/> is automatically called when an option is changed.
/// </para>
/// </remarks>
public partial class Configuration : BindableBase, IConfiguration
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

    private uint _cps;
    private bool _cpsIncludesWhitespace;
    private bool _cpsIncludesPunctuation;
    private bool _autosaveEnabled;
    private uint _autosaveInterval;
    private bool _useSoftLinebreaks;
    private bool _lineWidthIncludesWhitespace;
    private bool _lineWidthIncludesPunctuation;
    private bool _discordRpcEnabled;
    private int _defaultLayer;
    private string _culture;
    private string _spellcheckCulture;
    private Theme _theme;
    private uint _gridPadding;
    private RangeObservableCollection<string> _repositoryUrls;

    /// <inheritdoc />
    public uint Cps
    {
        get => _cps;
        set => SetProperty(ref _cps, value);
    }

    /// <inheritdoc />
    public bool CpsIncludesWhitespace
    {
        get => _cpsIncludesWhitespace;
        set => SetProperty(ref _cpsIncludesWhitespace, value);
    }

    /// <inheritdoc />
    public bool CpsIncludesPunctuation
    {
        get => _cpsIncludesPunctuation;
        set => SetProperty(ref _cpsIncludesPunctuation, value);
    }

    /// <inheritdoc />
    public bool UseSoftLinebreaks
    {
        get => _useSoftLinebreaks;
        set => SetProperty(ref _useSoftLinebreaks, value);
    }

    /// <inheritdoc />
    public int DefaultLayer
    {
        get => _defaultLayer;
        set => SetProperty(ref _defaultLayer, value);
    }

    /// <inheritdoc />
    public bool AutosaveEnabled
    {
        get => _autosaveEnabled;
        set => SetProperty(ref _autosaveEnabled, value);
    }

    /// <inheritdoc />
    public uint AutosaveInterval
    {
        get => _autosaveInterval;
        set => SetProperty(ref _autosaveInterval, value);
    }

    /// <inheritdoc />
    public bool LineWidthIncludesWhitespace
    {
        get => _lineWidthIncludesWhitespace;
        set => SetProperty(ref _lineWidthIncludesWhitespace, value);
    }

    /// <inheritdoc />
    public bool LineWidthIncludesPunctuation
    {
        get => _lineWidthIncludesPunctuation;
        set => SetProperty(ref _lineWidthIncludesPunctuation, value);
    }

    /// <inheritdoc />
    public bool DiscordRpcEnabled
    {
        get => _discordRpcEnabled;
        set => SetProperty(ref _discordRpcEnabled, value);
    }

    /// <inheritdoc />
    public string Culture
    {
        get => _culture;
        set => SetProperty(ref _culture, value);
    }

    /// <inheritdoc />
    public string SpellcheckCulture
    {
        get => _spellcheckCulture;
        set => SetProperty(ref _spellcheckCulture, value);
    }

    /// <inheritdoc />
    public Theme Theme
    {
        get => _theme;
        set => SetProperty(ref _theme, value);
    }

    /// <inheritdoc />
    public uint GridPadding
    {
        get => _gridPadding;
        set => SetProperty(ref _gridPadding, value);
    }

    /// <inheritdoc />
    public ReadOnlyObservableCollection<string> RepositoryUrls { get; }

    /// <inheritdoc />
    public void AddRepositoryUrl(string url)
    {
        Logger.Debug($"Adding repository url {url}");
        _repositoryUrls.Add(url);
        Save();
    }

    /// <inheritdoc />
    public bool RemoveRepositoryUrl(string url)
    {
        Logger.Debug($"Removing repository url {url}");
        var result = _repositoryUrls.Remove(url);
        Save();
        return result;
    }

    /// <inheritdoc />
    public bool Save()
    {
        var path = Paths.Configuration.LocalPath;
        Logger.Info($"Writing configuration to {path}...");
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

            var model = new ConfigurationModel
            {
                Version = ConfigurationModel.CurrentApiVersion,
                Cps = _cps,
                CpsIncludesWhitespace = _cpsIncludesWhitespace,
                CpsIncludesPunctuation = _cpsIncludesPunctuation,
                UseSoftLinebreaks = _useSoftLinebreaks,
                AutosaveEnabled = _autosaveEnabled,
                AutosaveInterval = _autosaveInterval,
                LineWidthIncludesWhitespace = _lineWidthIncludesWhitespace,
                LineWidthIncludesPunctuation = _lineWidthIncludesPunctuation,
                DiscordRpcEnabled = _discordRpcEnabled,
                DefaultLayer = _defaultLayer,
                Culture = _culture,
                SpellcheckCulture = _spellcheckCulture,
                Theme = _theme,
                GridPadding = _gridPadding,
                RepositoryUrls = RepositoryUrls.ToArray(),
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

    /// <summary>
    /// Parse a saved configuration file
    /// </summary>
    /// <param name="fileSystem">FileSystem to use</param>
    /// <returns><see cref="Configuration"/> object</returns>
    public static Configuration Parse(IFileSystem fileSystem)
    {
        Logger.Info("Parsing configuration...");
        var path = Paths.Configuration.LocalPath;
        try
        {
            if (!fileSystem.Directory.Exists(Path.GetDirectoryName(path)))
                fileSystem.Directory.CreateDirectory(Path.GetDirectoryName(path) ?? "/");

            if (!fileSystem.File.Exists(path))
            {
                Logger.Warn("Configuration file does not exist, using defaults...");
                return new Configuration(fileSystem);
            }

            using var fs = fileSystem.FileStream.New(
                path,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite
            );
            using var reader = new StreamReader(fs);

            var model =
                JsonSerializer.Deserialize<ConfigurationModel>(reader.ReadToEnd(), JsonOptions)
                ?? throw new InvalidDataException("Configuration model deserialization failed!");

            var result = new Configuration(fileSystem)
            {
                _cps = model.Cps,
                _cpsIncludesWhitespace = model.CpsIncludesWhitespace,
                _cpsIncludesPunctuation = model.CpsIncludesPunctuation,
                _useSoftLinebreaks = model.UseSoftLinebreaks,
                _autosaveEnabled = model.AutosaveEnabled,
                _autosaveInterval = model.AutosaveInterval,
                _lineWidthIncludesWhitespace = model.LineWidthIncludesWhitespace,
                _lineWidthIncludesPunctuation = model.LineWidthIncludesPunctuation,
                _discordRpcEnabled = model.DiscordRpcEnabled,
                _defaultLayer = model.DefaultLayer,
                _culture = model.Culture,
                _spellcheckCulture = model.SpellcheckCulture,
                _theme = model.Theme,
                _gridPadding = model.GridPadding,
                _repositoryUrls = new RangeObservableCollection<string>(model.RepositoryUrls),
            };
            Logger.Info("Done!");
            return result;
        }
        catch (Exception ex) when (ex is IOException or JsonException)
        {
            Logger.Error(ex);
            Logger.Error("Failed to parse configuration, using defaults...");
            return new Configuration(fileSystem);
        }
    }

    /// <summary>
    /// Instantiate a Configuration instance
    /// </summary>
    /// <param name="fileSystem">FileSystem to use</param>
    public Configuration(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
        _cps = 18;
        _useSoftLinebreaks = false;
        _discordRpcEnabled = true;
        _autosaveEnabled = true;
        _autosaveInterval = 60;
        _repositoryUrls = [];
        _culture = "en-US";
        _theme = Theme.Default;
        _gridPadding = 2;

        RepositoryUrls = new ReadOnlyObservableCollection<string>(_repositoryUrls);
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

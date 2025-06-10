// SPDX-License-Identifier: MPL-2.0

using System.Collections.ObjectModel;
using System.IO.Abstractions;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using AssCS;
using Holo.Models;
using NLog;

namespace Holo;

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
public partial class Configuration : BindableBase
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
    /// <remarks>This allows for filesystem mocking to be used in tests</remarks>
    private readonly IFileSystem _fileSystem;

    private int _cps;
    private bool _cpsIncludesWhitespace;
    private bool _cpsIncludesPunctuation;
    private bool _autosaveEnabled;
    private int _autosaveInterval;
    private bool _useSoftLinebreaks;
    private bool _lineWidthIncludesWhitespace;
    private bool _lineWidthIncludesPunctuation;
    private string _culture;
    private Theme _theme;
    private RangeObservableCollection<string> _repositoryUrls;

    /// <summary>
    /// Characters-per-second threshold
    /// </summary>
    /// <remarks>This value may be overloaded by <see cref="Solution.Cps"/>.</remarks>
    public int Cps
    {
        get => _cps;
        set => SetProperty(ref _cps, value);
    }

    /// <summary>
    /// If whitespace should be included in <see cref="Event.Cps"/> calculation
    /// </summary>
    public bool CpsIncludesWhitespace
    {
        get => _cpsIncludesWhitespace;
        set => SetProperty(ref _cpsIncludesWhitespace, value);
    }

    /// <summary>
    /// If punctuation should be included in <see cref="Event.Cps"/> calculation
    /// </summary>
    public bool CpsIncludesPunctuation
    {
        get => _cpsIncludesPunctuation;
        set => SetProperty(ref _cpsIncludesPunctuation, value);
    }

    /// <summary>
    /// Soft linebreaks preference
    /// </summary>
    /// <remarks>This value may be overloaded by <see cref="Solution.UseSoftLinebreaks"/>.</remarks>
    public bool UseSoftLinebreaks
    {
        get => _useSoftLinebreaks;
        set => SetProperty(ref _useSoftLinebreaks, value);
    }

    /// <summary>
    /// Whether autosave is enabled
    /// </summary>
    public bool AutosaveEnabled
    {
        get => _autosaveEnabled;
        set => SetProperty(ref _autosaveEnabled, value);
    }

    /// <summary>
    /// Interval between autosave attempts, in seconds
    /// </summary>
    public int AutosaveInterval
    {
        get => _autosaveInterval;
        set => SetProperty(ref _autosaveInterval, value);
    }

    /// <summary>
    /// If whitespace should be included in <see cref="Event.MaxLineWidth"/> calculation
    /// </summary>
    public bool LineWidthIncludesWhitespace
    {
        get => _lineWidthIncludesWhitespace;
        set => SetProperty(ref _lineWidthIncludesWhitespace, value);
    }

    /// <summary>
    /// If punctuation should be included in <see cref="Event.MaxLineWidth"/> calculation
    /// </summary>
    public bool LineWidthIncludesPunctuation
    {
        get => _lineWidthIncludesPunctuation;
        set => SetProperty(ref _lineWidthIncludesPunctuation, value);
    }

    public string Culture
    {
        get => _culture;
        set => SetProperty(ref _culture, value);
    }

    /// <summary>
    /// Display theme to use
    /// </summary>
    public Theme Theme
    {
        get => _theme;
        set => SetProperty(ref _theme, value);
    }

    /// <summary>
    /// List of user-added repository URLs
    /// </summary>
    public ReadOnlyObservableCollection<string> RepositoryUrls { get; }

    public Uri SavePath { get; }

    /// <summary>
    /// Add a repository
    /// </summary>
    /// <param name="url">Repository to add</param>
    public void AddRepositoryUrl(string url)
    {
        Logger.Info($"Adding repository url {url}");
        _repositoryUrls.Add(url);
        Save();
    }

    /// <summary>
    /// Remove a repository
    /// </summary>
    /// <param name="url">Repository to remove</param>
    /// <returns><see langword="true"/> if successful</returns>
    public bool RemoveRepositoryUrl(string url)
    {
        Logger.Info($"Removing repository url {url}");
        var result = _repositoryUrls.Remove(url);
        Save();
        return result;
    }

    /// <summary>
    /// Write the solution to file
    /// </summary>
    /// <returns><see langword="true"/> if saving was successful</returns>
    public bool Save()
    {
        var path = SavePath.LocalPath;
        Logger.Info($"Writing configuration to {path}");
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
                Culture = _culture,
                Theme = _theme,
                RepositoryUrls = RepositoryUrls.ToArray(),
            };

            var content = JsonSerializer.Serialize(model, JsonOptions);
            writer.Write(content);
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
    /// <param name="savePath">Path to the configuration file</param>
    /// <returns><see cref="Configuration"/> object</returns>
    public static Configuration Parse(Uri savePath)
    {
        return Parse(new FileSystem(), savePath);
    }

    /// <summary>
    /// Parse a saved configuration file
    /// </summary>
    /// <param name="fileSystem">FileSystem to use</param>
    /// <param name="savePath">Path to the configuration file</param>
    /// <returns><see cref="Configuration"/> object</returns>
    public static Configuration Parse(IFileSystem fileSystem, Uri savePath)
    {
        Logger.Info("Parsing configuration");
        var path = savePath.LocalPath;
        try
        {
            if (!fileSystem.Directory.Exists(Path.GetDirectoryName(path)))
                fileSystem.Directory.CreateDirectory(Path.GetDirectoryName(path) ?? "/");

            if (!fileSystem.File.Exists(path))
                return new Configuration(fileSystem, savePath);

            using var fs = fileSystem.FileStream.New(
                path,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite
            );
            using var reader = new StreamReader(fs);

            var model =
                JsonSerializer.Deserialize<ConfigurationModel>(reader.ReadToEnd(), JsonOptions)
                ?? throw new InvalidDataException("Configuration model deserialization failed");

            return new Configuration(fileSystem, savePath)
            {
                _cps = model.Cps,
                _cpsIncludesWhitespace = model.CpsIncludesWhitespace,
                _cpsIncludesPunctuation = model.CpsIncludesPunctuation,
                _useSoftLinebreaks = model.UseSoftLinebreaks,
                _autosaveEnabled = model.AutosaveEnabled,
                _autosaveInterval = model.AutosaveInterval,
                _lineWidthIncludesWhitespace = model.LineWidthIncludesWhitespace,
                _lineWidthIncludesPunctuation = model.LineWidthIncludesPunctuation,
                _culture = model.Culture,
                _theme = model.Theme,
                _repositoryUrls = new RangeObservableCollection<string>(model.RepositoryUrls),
            };
        }
        catch (Exception ex) when (ex is IOException or JsonException)
        {
            Logger.Error(ex);
            return new Configuration(fileSystem, savePath);
        }
    }

    /// <summary>
    /// Instantiate a Configuration instance
    /// </summary>
    /// <param name="savePath">Location of the configuration file</param>
    public Configuration(Uri savePath)
        : this(new FileSystem(), savePath) { }

    /// <summary>
    /// Instantiate a Configuration instance
    /// </summary>
    /// <param name="fileSystem">FileSystem to use</param>
    /// <param name="savePath">Location of the configuration file</param>
    public Configuration(IFileSystem fileSystem, Uri savePath)
    {
        SavePath = savePath;
        _fileSystem = fileSystem;
        _cps = 18;
        _useSoftLinebreaks = false;
        _autosaveEnabled = true;
        _autosaveInterval = 60;
        _repositoryUrls = [];
        _culture = "en-US";
        _theme = Theme.Default;

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

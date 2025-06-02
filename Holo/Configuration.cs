// SPDX-License-Identifier: MPL-2.0

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

    private int _cps;
    private bool _cpsIncludesWhitespace;
    private bool _cpsIncludesPunctuation;
    private bool _autosaveEnabled;
    private int _autosaveInterval;
    private bool _useSoftLinebreaks;
    private bool _lineWidthIncludesWhitespace;
    private bool _lineWidthIncludesPunctuation;
    private Theme _theme;

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

    /// <summary>
    /// Display theme to use
    /// </summary>
    public Theme Theme
    {
        get => _theme;
        set => SetProperty(ref _theme, value);
    }

    public Uri? SavePath { get; init; }

    /// <summary>
    /// Write the configuration to file
    /// </summary>
    /// <returns><see langword="true"/> if saving was successful</returns>
    /// <remarks>
    /// <see cref="SavePath"/> must be set prior to calling this method.
    /// </remarks>
    public bool Save()
    {
        if (string.IsNullOrEmpty(SavePath?.LocalPath))
            return false;

        Logger.Info($"Writing configuration to {SavePath.LocalPath}");
        var fp = SavePath.LocalPath;
        using var writer = new StreamWriter(fp, false);
        return Save(writer);
    }

    /// <summary>
    /// Write the solution to file
    /// </summary>
    /// <param name="writer">Writer to write to</param>
    /// <returns><see langword="true"/> if saving was successful</returns>
    public bool Save(TextWriter writer)
    {
        Logger.Info($"Saving configuration");

        try
        {
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
                Theme = _theme,
            };

            var content = JsonSerializer.Serialize(model, JsonOptions);
            writer.Write(content);
            return true;
        }
        catch (JsonException je)
        {
            Logger.Error(je);
            return false;
        }
        catch (IOException ioe)
        {
            Logger.Error(ioe);
            return false;
        }
    }

    /// <summary>
    /// Parse a saved configuration file
    /// </summary>
    /// <param name="filePath">Path to the configuration file</param>
    /// <returns><see cref="Configuration"/> object</returns>
    public static Configuration Parse(Uri filePath)
    {
        if (!File.Exists(filePath.LocalPath))
            return new Configuration { SavePath = filePath };

        using var reader = new StreamReader(filePath.LocalPath);
        return Parse(reader, filePath);
    }

    /// <summary>
    /// Parse a saved configuration file
    /// </summary>
    /// <param name="reader">Reader to read the configuration from</param>
    /// <param name="filePath">Path to the configuration file</param>
    /// <returns><see cref="Configuration"/> object</returns>
    /// <remarks>Returns the default configuration if <paramref name="reader"/> is empty.</remarks>
    public static Configuration Parse(TextReader reader, Uri? filePath)
    {
        if (reader.Peek() == -1)
            return new Configuration { SavePath = filePath };

        try
        {
            var model =
                JsonSerializer.Deserialize<ConfigurationModel>(reader.ReadToEnd(), JsonOptions)
                ?? throw new InvalidDataException("Configuration model serialization failed");

            return new Configuration
            {
                SavePath = filePath,
                _cps = model.Cps,
                _cpsIncludesWhitespace = model.CpsIncludesWhitespace,
                _cpsIncludesPunctuation = model.CpsIncludesPunctuation,
                _useSoftLinebreaks = model.UseSoftLinebreaks,
                _autosaveEnabled = model.AutosaveEnabled,
                _autosaveInterval = model.AutosaveInterval,
                _lineWidthIncludesWhitespace = model.LineWidthIncludesWhitespace,
                _lineWidthIncludesPunctuation = model.LineWidthIncludesPunctuation,
                _theme = model.Theme,
            };
        }
        catch (JsonException je)
        {
            Logger.Error(je);
            return new Configuration();
        }
        catch (IOException ioe)
        {
            Logger.Error(ioe);
            return new Configuration();
        }
    }

    public Configuration()
    {
        Cps = 18;
        UseSoftLinebreaks = false;
        AutosaveEnabled = true;
        AutosaveInterval = 60;
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

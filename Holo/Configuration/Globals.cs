// SPDX-License-Identifier: MPL-2.0

using System.Collections.ObjectModel;
using System.IO.Abstractions;
using System.Text.Json;
using AssCS;
using Holo.IO;
using Holo.Models;
using NLog;

namespace Holo.Configuration;

/// <summary>
/// Container for globally-accessible objects
/// </summary>
public class Globals : BindableBase, IGlobals
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private static readonly JsonSerializerOptions JsonOptions = new() { IncludeFields = true };

    private readonly ObservableCollection<Color> _colors;
    private readonly ObservableCollection<string> _customWords;

    /// <summary>
    /// The filesystem being used
    /// </summary>
    private readonly IFileSystem _fileSystem;

    /// <inheritdoc cref="IGlobals.StyleManager"/>
    public StyleManager StyleManager { get; }

    /// <inheritdoc cref="IGlobals.Colors"/>
    public AssCS.Utilities.ReadOnlyObservableCollection<Color> Colors { get; }

    /// <inheritdoc />
    public AssCS.Utilities.ReadOnlyObservableCollection<string> CustomWords { get; }

    /// <inheritdoc cref="IGlobals.AddColor"/>
    public bool AddColor(Color color)
    {
        if (_colors.Contains(color))
            return false;
        _colors.Add(color);
        return true;
    }

    /// <inheritdoc cref="IGlobals.RemoveColor"/>
    public bool RemoveColor(Color color)
    {
        if (!_colors.Contains(color))
            return false;
        _colors.Remove(color);
        return true;
    }

    /// <inheritdoc />
    public bool AddCustomWord(string word)
    {
        if (_customWords.Contains(word))
            return false;
        _customWords.Add(word);
        return true;
    }

    /// <inheritdoc />
    public bool RemoveCustomWord(string word)
    {
        return _customWords.Remove(word);
    }

    /// <inheritdoc cref="IGlobals.Save"/>
    public bool Save()
    {
        var path = Paths.Globals.LocalPath;
        Logger.Info($"Writing globals to {path}...");
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

            var model = new GlobalsModel
            {
                Version = GlobalsModel.CurrentApiVersion,
                Styles = StyleManager.Styles.Select(s => s.AsAss()).ToArray(),
                Colors = Colors.Select(s => s.AsStyleColor()).ToArray(),
                CustomWords = CustomWords.ToArray(),
            };

            var content = JsonSerializer.Serialize(model, JsonOptions);
            writer.Write(content);
            Logger.Info("Done!");
            return true;
        }
        catch (Exception ex) when (ex is IOException or JsonException)
        {
            Logger.Error(ex);
            return false;
        }
    }

    /// <summary>
    /// Parse a saved globals file
    /// </summary>
    /// <param name="fileSystem">FileSystem to use</param>
    /// <returns><see cref="Globals"/> object</returns>
    public static Globals Parse(IFileSystem fileSystem)
    {
        Logger.Info("Parsing globals...");
        var path = Paths.Globals.LocalPath;
        try
        {
            if (!fileSystem.Directory.Exists(Path.GetDirectoryName(path)))
                fileSystem.Directory.CreateDirectory(Path.GetDirectoryName(path) ?? "/");

            if (!fileSystem.File.Exists(path))
            {
                Logger.Warn("Globals file does not exist, using defaults...");
                return new Globals(fileSystem);
            }

            using var fs = fileSystem.FileStream.New(
                path,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite
            );
            using var reader = new StreamReader(fs);

            var model =
                JsonSerializer.Deserialize<GlobalsModel>(reader.ReadToEnd(), JsonOptions)
                ?? throw new InvalidDataException("Globals model deserialization failed");

            var g = new Globals(fileSystem);
            foreach (var style in model.Styles.Select(s => Style.FromAss(g.StyleManager.NextId, s)))
                g.StyleManager.Add(style);

            foreach (var color in model.Colors.Select(Color.FromAss))
                g._colors.Add(color);

            foreach (var word in model.CustomWords)
                g._customWords.Add(word);

            Logger.Info("Done!");
            return g;
        }
        catch (Exception ex) when (ex is IOException or JsonException)
        {
            Logger.Error(ex);
            Logger.Error("Failed to parse globals, using defaults...");
            return new Globals(fileSystem);
        }
    }

    /// <summary>
    /// Instantiate a Globals instance
    /// </summary>
    /// <param name="fileSystem">FileSystem to use</param>
    public Globals(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
        _colors = [];
        _customWords = [];

        StyleManager = new StyleManager();
        Colors = new AssCS.Utilities.ReadOnlyObservableCollection<Color>(_colors);
        CustomWords = new AssCS.Utilities.ReadOnlyObservableCollection<string>(_customWords);

        StyleManager.Styles.CollectionChanged += (_, _) => Save();
        Colors.CollectionChanged += (_, _) => Save();
    }
}

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

    /// <summary>
    /// The filesystem being used
    /// </summary>
    private readonly IFileSystem _fileSystem;

    /// <inheritdoc cref="IGlobals.StyleManager"/>
    public StyleManager StyleManager { get; }

    /// <inheritdoc cref="IGlobals.Colors"/>
    public AssCS.Utilities.ReadOnlyObservableCollection<Color> Colors { get; }

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

    /// <inheritdoc cref="IGlobals.Save"/>
    public bool Save()
    {
        var path = Paths.Globals.LocalPath;
        Logger.Info($"Writing globals to {path}");
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
    /// Parse a saved globals file
    /// </summary>
    /// <param name="fileSystem">FileSystem to use</param>
    /// <returns><see cref="Globals"/> object</returns>
    public static Globals Parse(IFileSystem fileSystem)
    {
        Logger.Info("Parsing globals");
        var path = Paths.Globals.LocalPath;
        try
        {
            if (!fileSystem.Directory.Exists(Path.GetDirectoryName(path)))
                fileSystem.Directory.CreateDirectory(Path.GetDirectoryName(path) ?? "/");

            if (!fileSystem.File.Exists(path))
                return new Globals(fileSystem);

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

            return g;
        }
        catch (Exception ex) when (ex is IOException or JsonException)
        {
            Logger.Error(ex);
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

        StyleManager = new StyleManager();
        Colors = new AssCS.Utilities.ReadOnlyObservableCollection<Color>(_colors);

        StyleManager.Styles.CollectionChanged += (_, _) => Save();
        Colors.CollectionChanged += (_, _) => Save();
    }
}

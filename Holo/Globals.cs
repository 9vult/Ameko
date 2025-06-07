// SPDX-License-Identifier: MPL-2.0

using System.Collections.ObjectModel;
using System.IO.Abstractions;
using System.Text.Json;
using AssCS;
using Holo.Models;
using NLog;

namespace Holo;

/// <summary>
/// Container for globally-accessible objects
/// </summary>
public class Globals : BindableBase
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private static readonly JsonSerializerOptions JsonOptions = new() { IncludeFields = true };

    private readonly ObservableCollection<Color> _colors;
    private readonly Uri _savePath;

    /// <summary>
    /// The filesystem being used
    /// </summary>
    /// <remarks>This allows for filesystem mocking to be used in tests</remarks>
    private readonly IFileSystem _fileSystem;

    /// <summary>
    /// The globals style manager
    /// </summary>
    public StyleManager StyleManager { get; }

    public readonly AssCS.Utilities.ReadOnlyObservableCollection<Color> Colors;

    /// <summary>
    /// Add a global color
    /// </summary>
    /// <param name="color">Color to add</param>
    /// <returns><see langword="true"/> if the color was added</returns>
    public bool AddColor(Color color)
    {
        if (_colors.Contains(color))
            return false;
        _colors.Add(color);
        return true;
    }

    /// <summary>
    /// Remove a global color
    /// </summary>
    /// <param name="color">Color to remove</param>
    /// <returns><see langword="true"/> if the color was removed</returns>
    public bool RemoveColor(Color color)
    {
        if (!_colors.Contains(color))
            return false;
        _colors.Remove(color);
        return true;
    }

    /// <summary>
    /// Write the globals to file
    /// </summary>
    /// <returns><see langword="true"/> if saving was successful</returns>
    public bool Save()
    {
        var path = _savePath.LocalPath;
        Logger.Info($"Writing globals to {path}");
        try
        {
            if (!_fileSystem.Directory.Exists(Path.GetDirectoryName(path)))
                _fileSystem.Directory.CreateDirectory(Path.GetDirectoryName(path) ?? "/");

            using var fs = _fileSystem.FileStream.New(path, FileMode.OpenOrCreate);
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
    /// <param name="savePath">Location of the globals file</param>
    /// <returns><see cref="Globals"/> object</returns>
    public static Globals Parse(Uri savePath)
    {
        return Parse(new FileSystem(), savePath);
    }

    /// <summary>
    /// Parse a saved globals file
    /// </summary>
    /// <param name="fileSystem">FileSystem to use</param>
    /// <param name="savePath">Location of the globals file</param>
    /// <returns><see cref="Globals"/> object</returns>
    public static Globals Parse(IFileSystem fileSystem, Uri savePath)
    {
        Logger.Info("Parsing globals");
        var path = savePath.LocalPath;
        try
        {
            if (!fileSystem.Directory.Exists(Path.GetDirectoryName(path)))
                fileSystem.Directory.CreateDirectory(Path.GetDirectoryName(path) ?? "/");

            if (!fileSystem.File.Exists(path))
                return new Globals(fileSystem, savePath);

            using var fs = fileSystem.FileStream.New(path, FileMode.OpenOrCreate);
            using var reader = new StreamReader(fs);

            var model =
                JsonSerializer.Deserialize<GlobalsModel>(reader.ReadToEnd(), JsonOptions)
                ?? throw new InvalidDataException("Globals model deserialization failed");

            var g = new Globals(fileSystem, savePath);
            foreach (var style in model.Styles.Select(s => Style.FromAss(g.StyleManager.NextId, s)))
                g.StyleManager.Add(style);

            foreach (var color in model.Colors.Select(Color.FromAss))
                g._colors.Add(color);

            return g;
        }
        catch (Exception ex) when (ex is IOException or JsonException)
        {
            Logger.Error(ex);
            return new Globals(fileSystem, savePath);
        }
    }

    /// <summary>
    /// Instantiate a Globals instance
    /// </summary>
    /// <param name="savePath">Location of the globals file</param>
    public Globals(Uri savePath)
        : this(new FileSystem(), savePath) { }

    /// <summary>
    /// Instantiate a Globals instance
    /// </summary>
    /// <param name="fileSystem">FileSystem to use</param>
    /// <param name="savePath">Location of the globals file</param>
    public Globals(IFileSystem fileSystem, Uri savePath)
    {
        _fileSystem = fileSystem;
        _savePath = savePath;
        _colors = [];

        StyleManager = new StyleManager();
        Colors = new AssCS.Utilities.ReadOnlyObservableCollection<Color>(_colors);

        StyleManager.Styles.CollectionChanged += (_, _) => Save();
        Colors.CollectionChanged += (_, _) => Save();
    }
}

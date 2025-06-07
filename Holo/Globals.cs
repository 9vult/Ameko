// SPDX-License-Identifier: MPL-2.0

using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using AssCS;
using AssCS.IO;
using Holo.IO;
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

    private readonly StyleManager _styleManager;
    private readonly ObservableCollection<Color> _colors;
    private readonly Uri _savePath;

    /// <summary>
    /// The globals style manager
    /// </summary>
    public StyleManager StyleManager => _styleManager;

    public ReadOnlyObservableCollection<Color> Colors;

    /// <summary>
    /// Write the globals to file
    /// </summary>
    /// <returns><see langword="true"/> if saving was successful</returns>
    public bool Save()
    {
        using var writer = new StreamWriter(_savePath.LocalPath, false);
        return Save(writer);
    }

    /// <summary>
    /// Write the globals to file
    /// </summary>
    /// <param name="writer">Writer to write to</param>
    /// <returns><see langword="true"/> if saving was successful</returns>
    public bool Save(TextWriter writer)
    {
        Logger.Info($"Saving globals file");

        try
        {
            var model = new GlobalsModel
            {
                Version = GlobalsModel.CurrentApiVersion,
                Styles = _styleManager.Styles.Select(s => s.AsAss()).ToArray(),
                Colors = _colors.Select(c => c.AsOverrideColor()).ToArray(),
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
    /// Parse a saved globals file
    /// </summary>
    /// <param name="filePath">File path</param>
    /// <returns><see cref="Globals"/> object</returns>
    public static Globals Parse(Uri filePath)
    {
        if (!File.Exists(filePath.LocalPath))
            return new Globals(filePath);

        using var reader = new StreamReader(filePath.LocalPath);
        return Parse(reader, filePath);
    }

    /// <summary>
    /// Parse a saved globals file
    /// </summary>
    /// <param name="reader">Reader to read the globals from</param>
    /// <param name="filePath">File path</param>
    /// <returns><see cref="Globals"/> object</returns>
    public static Globals Parse(TextReader reader, Uri filePath)
    {
        if (reader.Peek() == -1)
            return new Globals(filePath);

        try
        {
            var model =
                JsonSerializer.Deserialize<GlobalsModel>(reader.ReadToEnd(), JsonOptions)
                ?? throw new InvalidDataException("Globals model serialization failed");

            var sln = new Globals(filePath);
            model
                .Styles.Select(s => Style.FromAss(sln._styleManager.NextId, s))
                .ToList()
                .ForEach(sln._styleManager.Add);

            model.Colors.Select(Color.FromAss).ToList().ForEach(sln._colors.Add);
            return sln;
        }
        catch (JsonException je)
        {
            Logger.Error(je);
            return new Globals(filePath);
        }
        catch (IOException ioe)
        {
            Logger.Error(ioe);
            return new Globals(filePath);
        }
    }

    /// <summary>
    /// Initialize the globals
    /// </summary>
    public Globals(Uri savePath)
    {
        _savePath = savePath;
        _styleManager = new StyleManager();
        _colors = [];

        Colors = new ReadOnlyObservableCollection<Color>(_colors);

        StyleManager.Styles.CollectionChanged += (_, _) => Save();
    }
}

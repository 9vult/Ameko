// SPDX-License-Identifier: MPL-2.0

using System.Collections.ObjectModel;
using System.IO.Abstractions;
using AssCS;
using Holo.IO;
using Holo.Models;
using NLog;
using Tomlet;

namespace Holo.Providers;

public class LayoutProvider : BindableBase, ILayoutProvider
{
    private static readonly Uri LayoutsRoot = new(
        Path.Combine(DirectoryService.DataHome, "layouts")
    );
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private readonly IFileSystem _fileSystem;

    private readonly ObservableCollection<TabLayout> _layouts;
    private TabLayout _currentLayout;

    /// <inheritdoc />
    public TabLayout Current
    {
        get => _currentLayout;
        set
        {
            SetProperty(ref _currentLayout, value);
            OnLayoutChanged?.Invoke(this, new ILayoutProvider.LayoutChangedEventArgs(value));
        }
    }

    /// <inheritdoc />
    public AssCS.Utilities.ReadOnlyObservableCollection<TabLayout> Layouts { get; }

    /// <inheritdoc />
    public void Reload()
    {
        Logger.Info("Reloading Layouts...");
        if (!_fileSystem.Directory.Exists(LayoutsRoot.LocalPath))
            _fileSystem.Directory.CreateDirectory(LayoutsRoot.LocalPath);

        _layouts.Clear();

        foreach (var path in Directory.EnumerateFiles(LayoutsRoot.LocalPath, "*.toml"))
        {
            try
            {
                Logger.Info($"Loading Layout {path}...");

                using var readFs = _fileSystem.FileStream.New(
                    path,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.ReadWrite
                );
                using var reader = new StreamReader(readFs);

                var layout = TomletMain.To<TabLayout>(reader.ReadToEnd());
                _layouts.Add(layout);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }
        Logger.Info($"Reloaded {_layouts.Count} layouts");

        if (_layouts.Any(l => l.Name == Current.Name))
        {
            OnLayoutChanged?.Invoke(this, new ILayoutProvider.LayoutChangedEventArgs(Current));
            return;
        }

        if (_layouts.Count > 0)
        {
            Current = _layouts[0];
            OnLayoutChanged?.Invoke(this, new ILayoutProvider.LayoutChangedEventArgs(Current));
            return;
        }

        Logger.Info("No layouts loaded! Generating default layout...");
        var defaultLayout = DefaultLayout;
        _layouts.Add(defaultLayout);
        Current = defaultLayout;
        try
        {
            using var writeFs = _fileSystem.FileStream.New(
                Path.Combine(LayoutsRoot.LocalPath, "default.toml"),
                FileMode.Create,
                FileAccess.Write,
                FileShare.None
            );
            using var writer = new StreamWriter(writeFs);
            var content = TomletMain.TomlStringFrom(defaultLayout);
            writer.Write(content);
            Logger.Info("Done!");
            OnLayoutChanged?.Invoke(
                this,
                new ILayoutProvider.LayoutChangedEventArgs(defaultLayout)
            );
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    /// <inheritdoc />
    public event EventHandler<ILayoutProvider.LayoutChangedEventArgs>? OnLayoutChanged;

    /// <summary>
    /// Initialize the layout provider
    /// </summary>
    /// <param name="fileSystem">Filesystem to use</param>
    public LayoutProvider(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
        _layouts = [];
        Layouts = new AssCS.Utilities.ReadOnlyObservableCollection<TabLayout>(_layouts);

        Reload();

        _currentLayout = _layouts.FirstOrDefault(l => l.Name == "Default") ?? _layouts.First();
    }

    private static TabLayout DefaultLayout =>
        new()
        {
            Name = "Default",
            Author = "9volt",
            ColumnDefinitions = "*, 2, *",
            RowDefinitions = "0.5*, 2, *, 2, *",
            Video = new Section
            {
                IsVisible = true,
                Column = 0,
                Row = 0,
                RowSpan = 3,
            },
            Audio = new Section
            {
                IsVisible = true,
                Column = 2,
                Row = 0,
            },
            Editor = new Section
            {
                IsVisible = true,
                Column = 2,
                Row = 2,
            },
            Events = new Section
            {
                IsVisible = true,
                Column = 0,
                Row = 4,
                ColumnSpan = 3,
            },
            Splitters =
            [
                new Splitter
                {
                    IsVertical = false,
                    Column = 2,
                    Row = 1,
                },
                new Splitter
                {
                    IsVertical = false,
                    Column = 0,
                    Row = 3,
                    ColumnSpan = 3,
                },
                new Splitter
                {
                    IsVertical = true,
                    Column = 1,
                    Row = 0,
                    RowSpan = 3,
                },
            ],
        };
}

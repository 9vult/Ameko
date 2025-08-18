// SPDX-License-Identifier: MPL-2.0

using System.Collections.ObjectModel;
using System.IO.Abstractions;
using AssCS;
using Holo.Configuration;
using Holo.IO;
using Holo.Models;
using NLog;
using Tomlet;

namespace Holo.Providers;

public class LayoutProvider : BindableBase, ILayoutProvider
{
    private static readonly Uri LayoutsRoot = new(Path.Combine(Directories.DataHome, "layouts"));
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private readonly IFileSystem _fileSystem;
    private readonly IPersistence _persistence;

    private readonly ObservableCollection<Layout> _layouts;
    private Layout? _currentLayout;

    /// <inheritdoc />
    public Layout? Current
    {
        get => _currentLayout;
        set
        {
            if (value is null)
                return;
            SetProperty(ref _currentLayout, value);
            _persistence.LayoutName = value.Name;
            OnLayoutChanged?.Invoke(this, new ILayoutProvider.LayoutChangedEventArgs(value));
        }
    }

    /// <inheritdoc />
    public AssCS.Utilities.ReadOnlyObservableCollection<Layout> Layouts { get; }

    /// <inheritdoc />
    public void Reload()
    {
        Logger.Info("Reloading Layouts...");
        if (!_fileSystem.Directory.Exists(LayoutsRoot.LocalPath))
            _fileSystem.Directory.CreateDirectory(LayoutsRoot.LocalPath);

        _layouts.Clear();

        foreach (var path in _fileSystem.Directory.EnumerateFiles(LayoutsRoot.LocalPath, "*.toml"))
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

                var layout = TomletMain.To<Layout>(reader.ReadToEnd());
                _layouts.Add(layout);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }
        Logger.Info($"Reloaded {_layouts.Count} layouts");

        if (_layouts.Any(l => l.Name == _persistence.LayoutName))
        {
            _currentLayout = _layouts.First(l => l.Name == _persistence.LayoutName);
            OnLayoutChanged?.Invoke(this, new ILayoutProvider.LayoutChangedEventArgs(Current));
            return;
        }

        if (_layouts.Count > 0)
        {
            Current = _layouts[0];
            OnLayoutChanged?.Invoke(this, new ILayoutProvider.LayoutChangedEventArgs(Current));
            return;
        }

        Logger.Info("No layouts loaded! Generating default layouts...");

        foreach (
            var defaultLayout in new[]
            {
                (DefaultLayout, "default"),
                (DefaultRightSlnExplorer, "default-right"),
                (SubsOnlyLayout, "subs-only"),
                (PetzkuLayout, "petzku"),
            }
        )
        {
            var layout = defaultLayout.Item1;
            var fileName = $"{defaultLayout.Item2}.toml";
            _layouts.Add(layout);

            try
            {
                using var writeFs = _fileSystem.FileStream.New(
                    Path.Combine(LayoutsRoot.LocalPath, fileName),
                    FileMode.Create,
                    FileAccess.Write,
                    FileShare.None
                );
                using var writer = new StreamWriter(writeFs);
                var content = TomletMain.TomlStringFrom(layout);
                writer.Write(content);
                OnLayoutChanged?.Invoke(this, new ILayoutProvider.LayoutChangedEventArgs(layout));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
        Logger.Info("Done!");
        Current = _layouts.First(l => l.Name == DefaultLayout.Name);
    }

    /// <inheritdoc />
    public event EventHandler<ILayoutProvider.LayoutChangedEventArgs>? OnLayoutChanged;

    /// <summary>
    /// Initialize the layout provider
    /// </summary>
    /// <param name="fileSystem">Filesystem to use</param>
    /// <param name="persistence">Application persistence</param>
    public LayoutProvider(IFileSystem fileSystem, IPersistence persistence)
    {
        _fileSystem = fileSystem;
        _persistence = persistence;
        _layouts = [];
        Layouts = new AssCS.Utilities.ReadOnlyObservableCollection<Layout>(_layouts);
    }

    #region Default Layouts

    private static Layout DefaultLayout =>
        new()
        {
            Name = "Default",
            Author = "9volt",
            ColumnDefinitions = "*, 2, *",
            RowDefinitions = "0.5*, 2, *, 2, *",
            Window = new WindowSection { IsSolutionExplorerOnLeft = true },
            Video = new TabSection
            {
                IsVisible = true,
                Column = 0,
                Row = 0,
                RowSpan = 3,
            },
            Audio = new TabSection
            {
                IsVisible = true,
                Column = 2,
                Row = 0,
            },
            Editor = new TabSection
            {
                IsVisible = true,
                Column = 2,
                Row = 2,
            },
            Events = new TabSection
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
    private static Layout DefaultRightSlnExplorer =>
        new()
        {
            Name = "Default (Solution Explorer on Right)",
            Author = "9volt",
            ColumnDefinitions = "*, 2, *",
            RowDefinitions = "0.5*, 2, *, 2, *",
            Window = new WindowSection { IsSolutionExplorerOnLeft = false },
            Video = new TabSection
            {
                IsVisible = true,
                Column = 0,
                Row = 0,
                RowSpan = 3,
            },
            Audio = new TabSection
            {
                IsVisible = true,
                Column = 2,
                Row = 0,
            },
            Editor = new TabSection
            {
                IsVisible = true,
                Column = 2,
                Row = 2,
            },
            Events = new TabSection
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

    private static Layout SubsOnlyLayout =>
        new()
        {
            Name = "Subs Only",
            Author = "9volt",
            ColumnDefinitions = "*",
            RowDefinitions = "*, 2, 2*",
            Window = new WindowSection { IsSolutionExplorerOnLeft = true },
            Video = new TabSection
            {
                IsVisible = false,
                Column = 0,
                Row = 0,
            },
            Audio = new TabSection
            {
                IsVisible = false,
                Column = 0,
                Row = 0,
            },
            Editor = new TabSection
            {
                IsVisible = true,
                Column = 0,
                Row = 0,
            },
            Events = new TabSection
            {
                IsVisible = true,
                Column = 0,
                Row = 2,
            },
            Splitters =
            [
                new Splitter
                {
                    IsVertical = false,
                    Column = 0,
                    Row = 1,
                },
            ],
        };

    private static Layout PetzkuLayout =>
        new()
        {
            Name = "Petzku",
            Author = "9volt",
            ColumnDefinitions = "*, 2, *",
            RowDefinitions = "0.5*, 2, *, 2, *",
            Window = new WindowSection { IsSolutionExplorerOnLeft = true },
            Video = new TabSection
            {
                IsVisible = true,
                Column = 0,
                Row = 0,
                ColumnSpan = 3,
            },
            Audio = new TabSection
            {
                IsVisible = true,
                Column = 2,
                Row = 2,
            },
            Editor = new TabSection
            {
                IsVisible = true,
                Column = 0,
                Row = 2,
            },
            Events = new TabSection
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
                    Column = 0,
                    Row = 1,
                    ColumnSpan = 3,
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
                    Row = 2,
                },
            ],
        };

    #endregion Default Layouts
}

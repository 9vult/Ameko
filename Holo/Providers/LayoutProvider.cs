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

public class LayoutProvider : BindableBase
{
    private static readonly Uri LayoutsRoot = new(
        Path.Combine(DirectoryService.DataHome, "layouts")
    );
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private readonly IFileSystem _fileSystem;
    private readonly IConfiguration _configuration;

    private ObservableCollection<TabLayout> _layouts;
    private TabLayout _currentLayout;

    public TabLayout Current
    {
        get => _currentLayout;
        set => SetProperty(ref _currentLayout, value);
    }

    public AssCS.Utilities.ReadOnlyObservableCollection<TabLayout> Layouts;

    public void ReloadLayouts()
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

        if (_layouts.Count > 0)
            return;

        Logger.Info("No layouts loaded! Generating default layout...");
        _layouts.Add(DefaultLayout);
        try
        {
            using var writeFs = _fileSystem.FileStream.New(
                Path.Combine(LayoutsRoot.LocalPath, "default.toml"),
                FileMode.Create,
                FileAccess.Write,
                FileShare.None
            );
            using var writer = new StreamWriter(writeFs);
            var content = TomletMain.TomlStringFrom(DefaultLayout);
            writer.Write(content);
            Logger.Info("Done!");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public LayoutProvider(IFileSystem fileSystem, IConfiguration configuration)
    {
        _fileSystem = fileSystem;
        _configuration = configuration;
        _layouts = [];
        Layouts = new AssCS.Utilities.ReadOnlyObservableCollection<TabLayout>(_layouts);

        ReloadLayouts();

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
                IsVisible = false,
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

// SPDX-License-Identifier: MPL-2.0

using System.ComponentModel;
using System.IO.Abstractions.TestingHelpers;
using Holo.Configuration;
using Holo.IO;
using Holo.Providers;
using Holo.Tests.Utilities;
using Shouldly;

namespace Holo.Tests;

public class LayoutProviderTests
{
    [Fact]
    public void Constructor()
    {
        var fs = new MockFileSystem();
        var p = new MockPersistence();
        var provider = new LayoutProvider(fs, p);

        provider.Current.ShouldBeNull();
        provider.Layouts.ShouldBeEmpty();
    }

    [Fact]
    public void Reload()
    {
        var fs = new MockFileSystem();
        fs.AddFile(
            TestUtils
                .MakeTestableUri(
                    fs,
                    Path.Combine(DirectoryService.DataHome, "layouts", "testing.toml")
                )
                .LocalPath,
            new MockFileData(ExampleLayout)
        );
        var p = new MockPersistence();
        var provider = new LayoutProvider(fs, p);

        provider.Reload();

        provider.Current.ShouldNotBeNull();
        provider.Current.Name.ShouldBe("ThunderClan Camp");
        provider.Layouts.ShouldNotBeEmpty();
    }

    private const string ExampleLayout = """
        Name = "ThunderClan Camp"
        Author = "9volt"
        ColumnDefinitions = "*, 2, *"
        RowDefinitions = "0.5*, 2, *, 2, *"
        Window = { IsSolutionExplorerOnLeft = true }
        [Video]
        IsVisible = true
        Column = 0
        Row = 0
        ColumnSpan = 1
        RowSpan = 3
        [Audio]
        IsVisible = true
        Column = 2
        Row = 0
        ColumnSpan = 1
        RowSpan = 1
        [Editor]
        IsVisible = true
        Column = 2
        Row = 2
        ColumnSpan = 1
        RowSpan = 1
        [Events]
        IsVisible = true
        Column = 0
        Row = 4
        ColumnSpan = 3
        RowSpan = 1
        [[Splitters]]
        IsVertical = false
        Column = 2
        Row = 1
        ColumnSpan = 1
        RowSpan = 1
        """;
}

file class MockPersistence : IPersistence
{
    public string LayoutName { get; set; } = "ThunderClan Camp";
    public bool UseColorRing { get; set; } = true;

    public bool Save()
    {
        return true;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}

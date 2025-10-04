// SPDX-License-Identifier: MPL-2.0

using System.IO.Abstractions.TestingHelpers;
using Holo.Configuration;
using Holo.IO;
using Microsoft.Extensions.Logging.Abstractions;
using Shouldly;

namespace Holo.Tests;

public class PersistenceTests
{
    [Fact]
    public void Constructor()
    {
        var fs = new MockFileSystem();
        var lg = NullLogger<Persistence>.Instance;
        var p = new Persistence(fs, lg);

        p.LayoutName.ShouldBe("Default");
        p.UseColorRing.ShouldBeFalse();
    }

    [Fact]
    public void Parse_NotExists()
    {
        var fs = new MockFileSystem();
        var lg = NullLogger<Persistence>.Instance;
        var p = Persistence.Parse(fs, lg);

        p.LayoutName.ShouldBe("Default");
        p.UseColorRing.ShouldBeFalse();
    }

    [Fact]
    public void Parse_Exists()
    {
        var fs = new MockFileSystem(
            new Dictionary<string, MockFileData>
            {
                { Paths.Persistence.LocalPath, new MockFileData(ExamplePersistence) },
            }
        );
        var lg = NullLogger<Persistence>.Instance;
        var p = Persistence.Parse(fs, lg);

        p.LayoutName.ShouldBe("Upside Down");
        p.UseColorRing.ShouldBeTrue();
    }

    [Fact]
    public void Save_Exists()
    {
        var fs = new MockFileSystem(
            new Dictionary<string, MockFileData>
            {
                { Paths.Persistence.LocalPath, new MockFileData(ExamplePersistence) },
            }
        );
        var lg = NullLogger<Persistence>.Instance;
        var p = new Persistence(fs, lg) { LayoutName = "Wayside School" };

        var result = p.Save();

        result.ShouldBeTrue();
        fs.FileExists(Paths.Persistence.LocalPath).ShouldBeTrue();
    }

    [Fact]
    public void Save_NotExists()
    {
        var fs = new MockFileSystem();
        var lg = NullLogger<Persistence>.Instance;
        var p = new Persistence(fs, lg) { LayoutName = "Wayside School" };

        var result = p.Save();

        result.ShouldBeTrue();
        fs.FileExists(Paths.Persistence.LocalPath).ShouldBeTrue();
    }

    private const string ExamplePersistence = """
        {
            "Version": 1,
            "LayoutName": "Upside Down",
            "UseColorRing": true,
            "PlaygroundCs": "",
            "PlaygroundJs": ""
        }
        """;
}

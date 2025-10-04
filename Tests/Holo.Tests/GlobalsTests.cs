// SPDX-License-Identifier: MPL-2.0

using System.IO.Abstractions.TestingHelpers;
using AssCS;
using Holo.Configuration;
using Holo.IO;
using Microsoft.Extensions.Logging.Abstractions;
using Shouldly;

namespace Holo.Tests;

public class GlobalsTests
{
    [Fact]
    public void Constructor()
    {
        var fs = new MockFileSystem();
        var lg = NullLogger<Globals>.Instance;
        var g = new Globals(fs, lg);
        g.StyleManager.Count.ShouldBe(0);
        g.Colors.Count.ShouldBe(0);
    }

    [Fact]
    public void AddColor_NotExists()
    {
        var fs = new MockFileSystem();
        var lg = NullLogger<Globals>.Instance;
        var g = new Globals(fs, lg);

        var result = g.AddColor(Color.FromRgb(1, 2, 3));

        result.ShouldBeTrue();
        g.Colors.Count.ShouldBe(1);
    }

    [Fact]
    public void AddColor_Exists()
    {
        var fs = new MockFileSystem();
        var lg = NullLogger<Globals>.Instance;
        var g = new Globals(fs, lg);

        var result1 = g.AddColor(Color.FromRgb(1, 2, 3));
        var result2 = g.AddColor(Color.FromRgb(1, 2, 3));

        result1.ShouldBeTrue();
        result2.ShouldBeFalse();
        g.Colors.Count.ShouldBe(1);
    }

    [Fact]
    public void RemoveColor_NotExists()
    {
        var fs = new MockFileSystem();
        var lg = NullLogger<Globals>.Instance;
        var g = new Globals(fs, lg);

        var result = g.RemoveColor(Color.FromRgb(1, 2, 3));

        result.ShouldBeFalse();
        g.Colors.Count.ShouldBe(0);
    }

    [Fact]
    public void RemoveColor_Exists()
    {
        var fs = new MockFileSystem();
        var lg = NullLogger<Globals>.Instance;
        var g = new Globals(fs, lg);

        var result1 = g.AddColor(Color.FromRgb(1, 2, 3));
        var result2 = g.RemoveColor(Color.FromRgb(1, 2, 3));

        result1.ShouldBeTrue();
        result2.ShouldBeTrue();
        g.Colors.Count.ShouldBe(0);
    }

    [Fact]
    public void Parse_NotExists()
    {
        var fs = new MockFileSystem();
        var lg = NullLogger<Globals>.Instance;
        var g = Globals.Parse(fs, lg);

        g.ShouldNotBeNull();
        g.StyleManager.Count.ShouldBe(0);
        g.Colors.Count.ShouldBe(0);
    }

    [Fact]
    public void Parse_Exists_Empty()
    {
        var fs = new MockFileSystem(
            new Dictionary<string, MockFileData>
            {
                { Paths.Globals.LocalPath, new MockFileData(ExampleGlobals1) },
            }
        );
        var lg = NullLogger<Globals>.Instance;
        var g = Globals.Parse(fs, lg);

        g.ShouldNotBeNull();
        g.StyleManager.Count.ShouldBe(0);
        g.Colors.Count.ShouldBe(0);
    }

    [Fact]
    public void Parse_Exists_NotEmpty()
    {
        var fs = new MockFileSystem(
            new Dictionary<string, MockFileData>
            {
                { Paths.Globals.LocalPath, new MockFileData(ExampleGlobals2) },
            }
        );
        var lg = NullLogger<Globals>.Instance;
        var g = Globals.Parse(fs, lg);

        g.ShouldNotBeNull();
        g.StyleManager.Count.ShouldBe(1);
        g.Colors.Count.ShouldBe(1);

        g.StyleManager.Styles.First().FontFamily.ShouldBe("Volkhov");
        g.Colors.First().Red.ShouldBe<byte>(0xAA);
    }

    [Fact]
    public void Save_Exists()
    {
        var fs = new MockFileSystem(
            new Dictionary<string, MockFileData>
            {
                { Paths.Globals.LocalPath, new MockFileData(ExampleGlobals2) },
            }
        );
        var lg = NullLogger<Globals>.Instance;
        var g = Globals.Parse(fs, lg);

        var result = g.Save();

        result.ShouldBeTrue();
        fs.FileExists(Paths.Globals.LocalPath).ShouldBeTrue();
    }

    [Fact]
    public void Save_NotExists()
    {
        var fs = new MockFileSystem();
        var lg = NullLogger<Globals>.Instance;
        var g = Globals.Parse(fs, lg);

        var result = g.Save();

        result.ShouldBeTrue();
        fs.FileExists(Paths.Globals.LocalPath).ShouldBeTrue();
    }

    private const string ExampleGlobals1 = """
         {
             "Version": 1,
             "Styles": [],
             "Colors": [],
             "CustomWords": []
         }
        """;

    private const string ExampleGlobals2 = """
         {
             "Version": 1,
             "Styles": ["Style: Default,Volkhov,69,&H00FFFFFF,&H000000FF,&H00052030,&HA0052030,-1,0,0,0,100,105,0,0,1,4.75,2.1,2,275,275,60,1"],
             "Colors": ["&HFF00AA"],
             "CustomWords": ["xnopyt"]
         }
        """;
}

// SPDX-License-Identifier: MPL-2.0

using System.IO.Abstractions.TestingHelpers;
using AssCS;
using Holo.Configuration;
using Holo.IO;
using Microsoft.Extensions.Logging.Abstractions;

namespace Holo.Tests;

public class GlobalsTests
{
    [Test]
    public async Task Constructor()
    {
        var fs = new MockFileSystem();
        var lg = NullLogger<Globals>.Instance;
        var g = new Globals(fs, lg);

        await Assert.That(g.StyleManager.Count).IsEqualTo(0);
        await Assert.That(g.Colors.Count).IsEqualTo(0);
    }

    [Test]
    public async Task AddColor_NotExists()
    {
        var fs = new MockFileSystem();
        var lg = NullLogger<Globals>.Instance;
        var g = new Globals(fs, lg);

        var result = g.AddColor(Color.FromRgb(1, 2, 3));

        await Assert.That(result).IsTrue();
        await Assert.That(g.Colors.Count).IsEqualTo(1);
    }

    [Test]
    public async Task AddColor_Exists()
    {
        var fs = new MockFileSystem();
        var lg = NullLogger<Globals>.Instance;
        var g = new Globals(fs, lg);

        var result1 = g.AddColor(Color.FromRgb(1, 2, 3));
        var result2 = g.AddColor(Color.FromRgb(1, 2, 3));

        await Assert.That(result1).IsTrue();
        await Assert.That(result2).IsFalse();
        await Assert.That(g.Colors.Count).IsEqualTo(1);
    }

    [Test]
    public async Task RemoveColor_NotExists()
    {
        var fs = new MockFileSystem();
        var lg = NullLogger<Globals>.Instance;
        var g = new Globals(fs, lg);

        var result = g.RemoveColor(Color.FromRgb(1, 2, 3));

        await Assert.That(result).IsFalse();
        await Assert.That(g.Colors.Count).IsEqualTo(0);
    }

    [Test]
    public async Task RemoveColor_Exists()
    {
        var fs = new MockFileSystem();
        var lg = NullLogger<Globals>.Instance;
        var g = new Globals(fs, lg);

        var result1 = g.AddColor(Color.FromRgb(1, 2, 3));
        var result2 = g.RemoveColor(Color.FromRgb(1, 2, 3));

        await Assert.That(result1).IsTrue();
        await Assert.That(result2).IsTrue();
        await Assert.That(g.Colors.Count).IsEqualTo(0);
    }

    [Test]
    public async Task Parse_NotExists()
    {
        var fs = new MockFileSystem();
        var lg = NullLogger<Globals>.Instance;
        var g = Globals.Parse(fs, lg);

        await Assert.That(g).IsNotNull();
        await Assert.That(g.StyleManager.Count).IsEqualTo(0);
        await Assert.That(g.Colors.Count).IsEqualTo(0);
    }

    [Test]
    public async Task Parse_Exists_Empty()
    {
        var fs = new MockFileSystem(
            new Dictionary<string, MockFileData>
            {
                { Paths.Globals.LocalPath, new MockFileData(ExampleGlobals1) },
            }
        );
        var lg = NullLogger<Globals>.Instance;
        var g = Globals.Parse(fs, lg);

        await Assert.That(g).IsNotNull();
        await Assert.That(g.StyleManager.Count).IsEqualTo(0);
        await Assert.That(g.Colors.Count).IsEqualTo(0);
    }

    [Test]
    public async Task Parse_Exists_NotEmpty()
    {
        var fs = new MockFileSystem(
            new Dictionary<string, MockFileData>
            {
                { Paths.Globals.LocalPath, new MockFileData(ExampleGlobals2) },
            }
        );
        var lg = NullLogger<Globals>.Instance;
        var g = Globals.Parse(fs, lg);

        await Assert.That(g).IsNotNull();
        await Assert.That(g.StyleManager.Count).IsEqualTo(1);
        await Assert.That(g.Colors.Count).IsEqualTo(1);

        await Assert.That(g.StyleManager.Styles.First().FontFamily).IsEqualTo("Volkhov");
        await Assert.That(g.Colors.First().Red).IsEqualTo<byte>(0xAA);
    }

    [Test]
    public async Task Save_Exists()
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

        await Assert.That(result).IsTrue();
        await Assert.That(fs.FileExists(Paths.Globals.LocalPath)).IsTrue();
    }

    [Test]
    public async Task Save_NotExists()
    {
        var fs = new MockFileSystem();
        var lg = NullLogger<Globals>.Instance;
        var g = Globals.Parse(fs, lg);

        var result = g.Save();

        await Assert.That(result).IsTrue();
        await Assert.That(fs.FileExists(Paths.Globals.LocalPath)).IsTrue();
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

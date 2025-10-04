// SPDX-License-Identifier: MPL-2.0

using System.IO.Abstractions.TestingHelpers;
using Holo.IO;
using Holo.Models;
using Microsoft.Extensions.Logging.Abstractions;
using Shouldly;

namespace Holo.Tests;

public class ConfigurationTests
{
    [Fact]
    public void Constructor()
    {
        var fs = new MockFileSystem();
        var lg = NullLogger<Configuration.Configuration>.Instance;
        var cfg = new Configuration.Configuration(fs, lg);

        cfg.Cps.ShouldBe<uint>(18);
        cfg.UseSoftLinebreaks.ShouldBe(false);
        cfg.AutosaveEnabled.ShouldBe(true);
        cfg.AutosaveInterval.ShouldBe<uint>(60);
    }

    [Fact]
    public void Parse_NotExists()
    {
        var fs = new MockFileSystem();
        var lg = NullLogger<Configuration.Configuration>.Instance;
        var cfg = Configuration.Configuration.Parse(fs, lg);

        cfg.ShouldNotBeNull();
        cfg.Cps.ShouldBe<uint>(18);
        cfg.UseSoftLinebreaks.ShouldBe(false);
        cfg.AutosaveEnabled.ShouldBe(true);
        cfg.AutosaveInterval.ShouldBe<uint>(60);
    }

    [Fact]
    public void Parse_Exists()
    {
        var fs = new MockFileSystem(
            new Dictionary<string, MockFileData>
            {
                { Paths.Configuration.LocalPath, new MockFileData(ExampleConfiguration) },
            }
        );
        var lg = NullLogger<Configuration.Configuration>.Instance;
        var cfg = Configuration.Configuration.Parse(fs, lg);

        cfg.ShouldNotBeNull();
        cfg.Cps.ShouldBe<uint>(12);
        cfg.CpsIncludesWhitespace.ShouldBeFalse();
        cfg.CpsIncludesPunctuation.ShouldBeFalse();
        cfg.UseSoftLinebreaks.ShouldBe(true);
        cfg.AutosaveEnabled.ShouldBe(false);
        cfg.AutosaveInterval.ShouldBe<uint>(120);
        cfg.LineWidthIncludesWhitespace.ShouldBeTrue();
        cfg.LineWidthIncludesPunctuation.ShouldBeTrue();
        cfg.Theme.ShouldBe(Theme.Light);
    }

    [Fact]
    public void Save_Exists()
    {
        var fs = new MockFileSystem(
            new Dictionary<string, MockFileData>
            {
                { Paths.Configuration.LocalPath, new MockFileData(ExampleConfiguration) },
            }
        );
        var lg = NullLogger<Configuration.Configuration>.Instance;
        var cfg = new Configuration.Configuration(fs, lg) { Cps = 12 };

        var result = cfg.Save();

        result.ShouldBeTrue();
        fs.FileExists(Paths.Configuration.LocalPath).ShouldBeTrue();
    }

    [Fact]
    public void Save_NotExists()
    {
        var fs = new MockFileSystem();
        var lg = NullLogger<Configuration.Configuration>.Instance;
        var cfg = new Configuration.Configuration(fs, lg) { Cps = 12 };

        var result = cfg.Save();

        result.ShouldBeTrue();
        fs.FileExists(Paths.Configuration.LocalPath).ShouldBeTrue();
    }

    private const string ExampleConfiguration = """
        {
            "Version": 1,
            "Cps": 12,
            "CpsIncludesWhitespace": false,
            "CpsIncludesPunctuation": false,
            "UseSoftLinebreaks": true,
            "AutosaveEnabled": false,
            "SaveFrames": "WithSubtitles",
            "DefaultLayer": 0,
            "AutosaveInterval": 120,
            "LineWidthIncludesWhitespace": true,
            "LineWidthIncludesPunctuation": true,
            "DiscordRpcEnabled": true,
            "Culture": "en-US",
            "SpellcheckCulture": "en_US",
            "Theme": "light",
            "PropagateFields": "NonText",
            "GridPadding": 2,
            "RepositoryUrls": []
        }
        """;
}

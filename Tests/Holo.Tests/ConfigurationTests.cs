// SPDX-License-Identifier: MPL-2.0

using AssCS;
using Shouldly;

namespace Holo.Tests;

public class ConfigurationTests
{
    [Fact]
    public void Constructor()
    {
        var cfg = new Configuration();
        cfg.SavePath.ShouldBeNull();
    }

    [Fact]
    public void Save()
    {
        var cfg = new Configuration { Cps = 12 };

        var writer = new StringWriter();
        bool result = cfg.Save(writer);

        result.ShouldBeTrue();
        writer.ToString().ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public void Parse()
    {
        var sr = new StringReader(ExampleConfiguration);
        var cfg = Configuration.Parse(sr, null);

        cfg.Cps.ShouldBe(12);
        cfg.UseSoftLinebreaks.ShouldBe(true);
        cfg.AutosaveEnabled.ShouldBe(false);
        cfg.AutosaveInterval.ShouldBe(120);
    }

    private const string ExampleConfiguration = """
        {
            "Version": 1,
            "Cps": 12,
            "CpsIncludesWhitespace": false,
            "CpsIncludesPunctuation": false,
            "UseSoftLinebreaks": true,
            "AutosaveEnabled": false,
            "AutosaveInterval": 120,
            "LineWidthIncludesWhitespace": true,
            "LineWidthIncludesPunctuation": true
        }
        """;
}

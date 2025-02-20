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
        const string input =
            "{\"Version\":1,\"Cps\":12,\"UseSoftLinebreaks\":true,\"AutosaveEnabled\":false,\"AutosaveInterval\":120}";

        var sr = new StringReader(input);
        var cfg = Configuration.Parse(sr, null);

        cfg.Cps.ShouldBe(12);
        cfg.UseSoftLinebreaks.ShouldBe(true);
        cfg.AutosaveEnabled.ShouldBe(false);
        cfg.AutosaveInterval.ShouldBe(120);
    }
}

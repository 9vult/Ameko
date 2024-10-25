// SPDX-License-Identifier: MPL-2.0

using FluentAssertions;

namespace AssCS.Tests;

public class ScriptInfoManagerTests
{
    [Fact]
    public void LoadDefault()
    {
        var sim = new ScriptInfoManager();
        sim.LoadDefault();

        sim.Count.Should().Be(15);
    }

    [Fact]
    public void Set()
    {
        var sim = new ScriptInfoManager();
        sim.Set("test", "testvalue");
        sim.Set("test2", "testvalue2");

        sim.Get("test").Should().Be("testvalue");
        sim.Get("test2").Should().Be("testvalue2");
    }

    [Fact]
    public void Set_Again()
    {
        var sim = new ScriptInfoManager();
        sim.Set("test", "testvalue");
        sim.Set("test", "testvalue2");

        sim.Get("test").Should().Be("testvalue2");
    }

    [Fact]
    public void Remove()
    {
        var sim = new ScriptInfoManager();
        sim.LoadDefault();

        sim.Get("Title").Should().Be("Default File");
        sim.Remove("Title");
        sim.Get("Title").Should().BeNull();
    }

    [Fact]
    public void Clear()
    {
        var sim = new ScriptInfoManager();
        sim.LoadDefault();

        sim.Clear();
        sim.Count.Should().Be(0);
    }
}

// SPDX-License-Identifier: MPL-2.0

using Shouldly;

namespace AssCS.Tests;

public class ScriptInfoManagerTests
{
    [Fact]
    public void LoadDefault()
    {
        var sim = new ScriptInfoManager();
        sim.LoadDefault();

        sim.Count.ShouldBe(15);
    }

    [Fact]
    public void Set()
    {
        var sim = new ScriptInfoManager();
        sim.Set("test", "testvalue");
        sim.Set("test2", "testvalue2");

        sim.Get("test").ShouldBe("testvalue");
        sim.Get("test2").ShouldBe("testvalue2");
    }

    [Fact]
    public void Set_Again()
    {
        var sim = new ScriptInfoManager();
        sim.Set("test", "testvalue");
        sim.Set("test", "testvalue2");

        sim.Get("test").ShouldBe("testvalue2");
    }

    [Fact]
    public void Set_BangHeader()
    {
        var sim = new ScriptInfoManager();
        sim.Set("!", "testvalue");
        sim.Set("!", "testvalue2");

        sim.GetAll().ShouldContain(new KeyValuePair<string, string>("!", "testvalue"));
        sim.GetAll().ShouldContain(new KeyValuePair<string, string>("!", "testvalue2"));
    }

    [Fact]
    public void Remove()
    {
        var sim = new ScriptInfoManager();
        sim.LoadDefault();

        sim.Get("Title").ShouldBe("Default File");
        sim.Remove("Title");
        sim.Get("Title").ShouldBeNull();
    }

    [Fact]
    public void Clear()
    {
        var sim = new ScriptInfoManager();
        sim.LoadDefault();

        sim.Clear();
        sim.Count.ShouldBe(0);
    }
}

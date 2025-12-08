// SPDX-License-Identifier: MPL-2.0

namespace AssCS.Tests;

public class ScriptInfoManagerTests
{
    [Test]
    public async Task LoadDefault()
    {
        var sim = new ScriptInfoManager();
        sim.LoadDefault();

        await Assert.That(sim.Count).IsEqualTo(15);
    }

    [Test]
    public async Task Set()
    {
        var sim = new ScriptInfoManager();
        sim.Set("test", "testvalue");
        sim.Set("test2", "testvalue2");

        await Assert.That(sim.Get("test")).IsEqualTo("testvalue");
        await Assert.That(sim.Get("test2")).IsEqualTo("testvalue2");
    }

    [Test]
    public async Task Set_Again()
    {
        var sim = new ScriptInfoManager();
        sim.Set("test", "testvalue");
        sim.Set("test", "testvalue2");

        await Assert.That(sim.Get("test")).IsEqualTo("testvalue2");
    }

    [Test]
    public async Task Set_BangHeader()
    {
        var sim = new ScriptInfoManager();
        sim.Set("!", "testvalue");
        sim.Set("!", "testvalue2");

        await Assert
            .That(sim.GetAll())
            .Contains(new KeyValuePair<string, string>("!", "testvalue"));
        await Assert
            .That(sim.GetAll())
            .Contains(new KeyValuePair<string, string>("!", "testvalue2"));
    }

    [Test]
    public async Task Remove()
    {
        var sim = new ScriptInfoManager();
        sim.LoadDefault();

        await Assert.That(sim.Get("Title")).IsEqualTo("Default File");
        sim.Remove("Title");
        await Assert.That(sim.Get("Title")).IsNull();
    }

    [Test]
    public async Task Clear()
    {
        var sim = new ScriptInfoManager();
        sim.LoadDefault();

        sim.Clear();
        await Assert.That(sim.Count).IsEqualTo(0);
    }
}

// SPDX-License-Identifier: MPL-2.0

using Shouldly;

namespace AssCS.Tests;

public class GarbageManagerTests
{
    [Fact]
    public void StringValue()
    {
        var gm = new GarbageManager();
        gm.Set("test", "test");

        gm.GetString("test").ShouldBe("test");
    }

    [Fact]
    public void DoubleValue()
    {
        var gm = new GarbageManager();
        gm.Set("test", 1.5d);

        gm.GetDouble("test").ShouldBe(1.5d);
    }

    [Fact]
    public void DecimalValue()
    {
        var gm = new GarbageManager();
        gm.Set("test", 1.5m);

        gm.GetDecimal("test").ShouldBe(1.5m);
    }

    [Fact]
    public void IntValue()
    {
        var gm = new GarbageManager();
        gm.Set("test", 1);

        gm.GetInt("test").ShouldBe(1);
    }

    [Fact]
    public void BoolValue()
    {
        var gm = new GarbageManager();
        gm.Set("test", true);

        gm.GetBool("test").ShouldBeTrue();
    }

    [Fact]
    public void LoadDefault()
    {
        var gm = new GarbageManager();
        gm.LoadDefault();

        gm.Count.ShouldBe(6);
    }

    [Fact]
    public void Clear()
    {
        var gm = new GarbageManager();
        gm.LoadDefault();
        gm.Clear();

        gm.Count.ShouldBe(0);
    }

    [Fact]
    public void Contains()
    {
        var gm = new GarbageManager();
        gm.Contains("test").ShouldBeFalse();

        gm.Set("test", "test");
        gm.Contains("test").ShouldBeTrue();
    }
}

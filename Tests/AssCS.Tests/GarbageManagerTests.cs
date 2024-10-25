// SPDX-License-Identifier: MPL-2.0

using FluentAssertions;

namespace AssCS.Tests;

public class GarbageManagerTests
{
    [Fact]
    public void StringValue()
    {
        var gm = new GarbageManager();
        gm.Set("test", "test");

        gm.GetString("test").Should().Be("test");
    }

    [Fact]
    public void DoubleValue()
    {
        var gm = new GarbageManager();
        gm.Set("test", 1.5d);

        gm.GetDouble("test").Should().Be(1.5d);
    }

    [Fact]
    public void DecimalValue()
    {
        var gm = new GarbageManager();
        gm.Set("test", 1.5m);

        gm.GetDecimal("test").Should().Be(1.5m);
    }

    [Fact]
    public void IntValue()
    {
        var gm = new GarbageManager();
        gm.Set("test", 1);

        gm.GetInt("test").Should().Be(1);
    }

    [Fact]
    public void BoolValue()
    {
        var gm = new GarbageManager();
        gm.Set("test", true);

        gm.GetBool("test").Should().BeTrue();
    }

    [Fact]
    public void LoadDefault()
    {
        var gm = new GarbageManager();
        gm.LoadDefault();

        gm.Count.Should().Be(6);
    }

    [Fact]
    public void Clear()
    {
        var gm = new GarbageManager();
        gm.LoadDefault();
        gm.Clear();

        gm.Count.Should().Be(0);
    }

    [Fact]
    public void Contains()
    {
        var gm = new GarbageManager();
        gm.Contains("test").Should().BeFalse();

        gm.Set("test", "test");
        gm.Contains("test").Should().BeTrue();
    }
}

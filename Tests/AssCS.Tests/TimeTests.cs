// SPDX-License-Identifier: MPL-2.0

using Shouldly;

namespace AssCS.Tests;

public class TimeTests
{
    [Fact]
    public void FromAss()
    {
        var t = Time.FromAss("0:02:10.57");

        t.ShouldBe(Time.FromMillis(130570));
    }

    [Fact]
    public void FromAss_Malformed()
    {
        Action action = () => Time.FromAss("0:02:10;57");

        action
            .ShouldThrow<ArgumentException>()
            .Message.ShouldBe("Time: 0:02:10;57 is an invalid timecode.");
    }

    [Fact]
    public void AsAss()
    {
        Time t = Time.FromMillis(130570);

        t.AsAss().ShouldBe("0:02:10.57");
    }

    [Fact]
    public void UpdatableText()
    {
        Time t = new() { UpdatableText = "0:02:10.57" };

        t.UpdatableText.ShouldBe("0:02:10.57");
    }

    [Fact]
    public void UpdatableText_Malformed()
    {
        Time t = new();

        Action action = () => t.UpdatableText = "0:02:10;57";

        action
            .ShouldThrow<ArgumentException>()
            .Message.ShouldBe("Time: 0:02:10;57 is an invalid timecode.");
    }
}

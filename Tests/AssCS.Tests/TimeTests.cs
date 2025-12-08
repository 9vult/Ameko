// SPDX-License-Identifier: MPL-2.0

namespace AssCS.Tests;

public class TimeTests
{
    [Test]
    public async Task From_TimeSpan()
    {
        var timeSpan = TimeSpan.FromSeconds(1);
        var t = new Time(timeSpan);

        await Assert.That(t).IsEqualTo(Time.FromSeconds(1));
    }

    [Test]
    public async Task From_TimeSpan_Clamps_To_Zero()
    {
        var timeSpan = TimeSpan.FromSeconds(-1);
        var t = new Time(timeSpan);

        await Assert.That(t).IsEqualTo(Time.FromSeconds(0));
    }

    [Test]
    public async Task Operator_Clamps_To_Zero()
    {
        var t1 = new Time(TimeSpan.FromSeconds(1));
        var t2 = new Time(TimeSpan.FromSeconds(2));

        var t3 = t1 - t2;

        await Assert.That(t3).IsEqualTo(Time.FromSeconds(0));
    }

    [Test]
    public async Task FromAss()
    {
        var t = Time.FromAss("0:02:10.57");

        await Assert.That(t).IsEqualTo(Time.FromMillis(130570));
    }

    [Test]
    public async Task FromAss_Malformed()
    {
        await Assert
            .That(() => Time.FromAss("0:02:10;57"))
            .Throws<ArgumentException>()
            .WithMessage("Time: 0:02:10;57 is an invalid timecode.");
    }

    [Test]
    public async Task AsAss()
    {
        var t = Time.FromMillis(130570);

        await Assert.That(t.AsAss()).IsEqualTo("0:02:10.57");
    }

    [Test]
    public async Task UpdatableText()
    {
        Time t = new() { UpdatableText = "0:02:10.57" };

        await Assert.That(t.UpdatableText).IsEqualTo("0:02:10.57");
    }

    [Test]
    public async Task UpdatableText_Malformed()
    {
        Time t = new();

        await Assert
            .That(() => t.UpdatableText = "0:02:10;57")
            .Throws<ArgumentException>()
            .WithMessage("Time: 0:02:10;57 is an invalid timecode.");
    }
}

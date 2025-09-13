// SPDX-License-Identifier: MPL-2.0

using Holo.Media;
using Shouldly;

namespace Holo.Tests;

public class VideoInfoTests
{
    private static VideoInfo Info =>
        new(
            "video.mkv",
            12,
            new Rational { Numerator = 16, Denominator = 9 },
            [0, 42, 83, 125, 167, 209, 250, 292, 334, 375, 417, 459],
            [42, 41, 42, 42, 42, 41, 42, 42, 41, 42, 42, 41],
            [0],
            1920,
            1080
        );

    [Fact]
    public void FrameFromMillis_Exact()
    {
        var info = Info;
        var frame = info.FrameFromMillis(125);
        frame.ShouldBe(3);
    }

    [Fact]
    public void FrameFromMillis_Between()
    {
        var info = Info;
        var frame = info.FrameFromMillis(126);
        frame.ShouldBe(4);
    }

    [Fact]
    public void FrameFromMillis_OutOfRange_Small()
    {
        var info = Info;
        var frame = info.FrameFromMillis(-5);
        frame.ShouldBe(0);
    }

    [Fact]
    public void FrameFromMillis_OutOfRange_Large()
    {
        var info = Info;
        var frame = info.FrameFromMillis(5000);
        frame.ShouldBe(11);
    }

    [Fact]
    public void MillisecondsFromFrame()
    {
        var info = Info;
        var ms = info.MillisecondsFromFrame(3);
        ms.ShouldBe(125);
    }

    [Fact]
    public void MillisecondsFromFrame_OutOfRange_Small()
    {
        var info = Info;
        var ms = info.MillisecondsFromFrame(-5);
        ms.ShouldBe(0);
    }

    [Fact]
    public void MillisecondsFromFrame_OutOfRange_Large()
    {
        var info = Info;
        var ms = info.MillisecondsFromFrame(5000);
        ms.ShouldBe(459);
    }
}

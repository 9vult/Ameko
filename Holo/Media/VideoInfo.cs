// SPDX-License-Identifier: MPL-2.0

using AssCS;

namespace Holo.Media;

public class VideoInfo(
    int frameCount,
    Rational sar,
    int[] frameTimes,
    int[] frameIntervals,
    int[] keyframes,
    int width,
    int height
)
{
    public int FrameCount { get; } = frameCount;
    public Rational Sar { get; init; } = sar;
    public int[] FrameTimes { get; } = frameTimes;
    public int[] FrameIntervals { get; } = frameIntervals;
    public int[] Keyframes { get; } = keyframes;
    public int Width { get; } = width;
    public int Height { get; } = height;

    /// <summary>
    /// Gets the index of the frame with the largest timestamp ≤ <paramref name="millis"/>.
    /// </summary>
    /// <remarks>
    /// If the time is out of bounds, returns the closest frame (0 or <see cref="FrameCount"/>)
    /// </remarks>
    /// <param name="millis">Time in milliseconds</param>
    /// <returns>Frame number</returns>
    public int FrameFromMillis(int millis)
    {
        if (millis < FrameTimes[0])
            return 0;
        if (millis > FrameTimes[^1])
            return FrameTimes.Length - 1;

        var bs = Array.BinarySearch(FrameTimes, millis);

        if (bs >= 0)
            return bs;

        // ~bs → Index of the first greater element. Subtract one for the smaller element
        return ~bs - 1;
    }

    /// <summary>
    /// Gets the time in milliseconds of the provided <paramref name="frameNumber"/>
    /// </summary>
    /// <remarks>
    /// If the frame number is out of bounds, returns the closest time
    /// </remarks>
    /// <param name="frameNumber">Frame number</param>
    /// <returns>Time in milliseconds</returns>
    public int MillisecondsFromFrame(int frameNumber)
    {
        return FrameTimes[Math.Clamp(frameNumber, 0, FrameTimes.Length - 1)];
    }

    /// <summary>
    /// Gets the index of the frame with the largest timestamp ≤ <paramref name="time"/>.
    /// </summary>
    /// <remarks>
    /// If the time is out of bounds, returns the closest frame (0 or <see cref="FrameCount"/>)
    /// </remarks>
    /// <param name="time">Time</param>
    /// <returns>Frame number</returns>
    public int FrameFromTime(Time time)
    {
        return FrameFromMillis((int)time.TotalMilliseconds);
    }

    /// <summary>
    /// Gets the time of the provided <paramref name="frameNumber"/>
    /// </summary>
    /// <remarks>
    /// If the frame number is out of bounds, returns the closest time
    /// </remarks>
    /// <param name="frameNumber">Frame number</param>
    /// <returns>Time</returns>
    public Time TimeFromFrame(int frameNumber)
    {
        return Time.FromMillis(MillisecondsFromFrame(frameNumber));
    }
}

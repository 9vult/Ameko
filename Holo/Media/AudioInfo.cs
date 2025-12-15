// SPDX-License-Identifier: MPL-2.0

namespace Holo.Media;

public class AudioInfo(int channelCount, int sampleRate, long sampleCount)
{
    public int ChannelCount { get; } = channelCount;
    public int SampleRate { get; } = sampleRate;
    public long SampleCount { get; } = sampleCount;
    public long Duration => (long)(1000.0 * SampleCount / SampleRate);
}

// SPDX-License-Identifier: MPL-2.0

using Holo.Media;
using Holo.Media.Providers;

namespace TestingUtils;

public unsafe class NullSourceProvider : ISourceProvider
{
    /// <inheritdoc />
    public bool IsInitialized => true;

    /// <inheritdoc />
    public int FrameCount => 0;

    /// <inheritdoc />
    public Rational Sar => new();

    /// <inheritdoc />
    public int Initialize()
    {
        return 0;
    }

    /// <inheritdoc />
    public int LoadVideo(
        string filename,
        ISourceProvider.IndexingProgressCallback? progressCallback = null
    )
    {
        return 0;
    }

    /// <inheritdoc />
    public int CloseVideo()
    {
        return 0;
    }

    /// <inheritdoc />
    public int SetSubtitles(string data, string? codePage)
    {
        return 0;
    }

    /// <inheritdoc />
    public int AllocateBuffers(int numBuffers, int maxCacheSize)
    {
        return 0;
    }

    /// <inheritdoc />
    public int AllocateAudioBuffer()
    {
        return 0;
    }

    /// <inheritdoc />
    public int FreeBuffers()
    {
        return 0;
    }

    /// <inheritdoc />
    public unsafe FrameGroup* GetFrame(int frameNumber, long timestamp, bool raw)
    {
        return null;
    }

    /// <inheritdoc />
    public unsafe AudioFrame* GetAudio(
        ISourceProvider.IndexingProgressCallback? progressCallback = null
    )
    {
        return null;
    }

    /// <inheritdoc />
    public Bitmap* GetVisualization(
        int width,
        int height,
        float pixelsPerMs,
        float amplitudeScale,
        long startTime,
        long frameTime
    )
    {
        return null;
    }

    /// <inheritdoc />
    public unsafe int ReleaseFrame(FrameGroup* frame)
    {
        return 0;
    }

    /// <inheritdoc />
    public int[] GetKeyframes()
    {
        return [];
    }

    /// <inheritdoc />
    public long[] GetTimecodes()
    {
        return [];
    }

    /// <inheritdoc />
    public long[] GetFrameIntervals()
    {
        return [];
    }

    /// <inheritdoc />
    public int GetChannelCount()
    {
        return 2;
    }

    /// <inheritdoc />
    public int GetSampleRate()
    {
        return 1;
    }

    /// <inheritdoc />
    public long GetSampleCount()
    {
        return 1;
    }
}

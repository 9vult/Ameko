// SPDX-License-Identifier: MPL-2.0

using System.Runtime.InteropServices;

namespace Holo.Media.Providers;

public interface ISourceProvider
{
    /// <summary>
    /// Whether this provider is initialized
    /// </summary>
    bool IsInitialized { get; }

    /// <summary>
    /// The number of frames in the loaded video
    /// </summary>
    int FrameCount { get; }

    /// <summary>
    /// Screen Aspect Ratio of the loaded video
    /// </summary>
    Rational Sar { get; }

    /// <summary>
    /// Initialize the provider
    /// </summary>
    /// <returns>0 on success</returns>
    int Initialize();

    /// <summary>
    /// Load a video
    /// </summary>
    /// <param name="filename">Path to the video to load</param>
    /// <param name="progressCallback">Indexing progress callback</param>
    /// <returns>0 on success</returns>
    int LoadVideo(string filename, IndexingProgressCallback? progressCallback = null);

    /// <summary>
    /// Close the open video
    /// </summary>
    /// <returns>0 on success</returns>
    int CloseVideo();

    /// <summary>
    /// Set the subtitles to be parsed
    /// </summary>
    /// <param name="data">Subtitle data</param>
    /// <param name="codePage">Codepage to use</param>
    /// <returns>0 on success</returns>
    int SetSubtitles(string data, string? codePage);

    /// <summary>
    /// Allocate frame buffers
    /// </summary>
    /// <param name="numBuffers">Number of buffers to pre-allocate</param>
    /// <param name="maxCacheSize">Maximum cache size in megabytes</param>
    /// <returns>0 on success</returns>
    int AllocateBuffers(int numBuffers, int maxCacheSize);

    /// <summary>
    /// Allocate audio buffer
    /// </summary>
    /// <returns>0 on success</returns>
    int AllocateAudioBuffer();

    /// <summary>
    /// Free frame buffers
    /// </summary>
    /// <returns>0 on success</returns>
    int FreeBuffers();

    /// <summary>
    /// Get a frame
    /// </summary>
    /// <param name="frameNumber">Frame number to get</param>
    /// <param name="timestamp">Timestamp to get</param>
    /// <param name="raw">Just the video</param>
    /// <returns>Output frame</returns>
    unsafe FrameGroup* GetFrame(int frameNumber, long timestamp, bool raw);

    /// <summary>
    /// Get the audio frame buffer
    /// </summary>
    /// <param name="progressCallback">Indexing progress callback</param>
    /// <returns>Audio frame</returns>
    unsafe AudioFrame* GetAudio(IndexingProgressCallback? progressCallback = null);

    /// <summary>
    /// Get a waveform bitmap for the specified time
    /// </summary>
    /// <param name="width">Width of the generated bitmap</param>
    /// <param name="height">Height of the generated bitmap</param>
    /// <param name="pixelsPerMs">Pixels per millisecond - Horizontal scale</param>
    /// <param name="amplitudeScale">Amplitude scale factor - Vertical scale</param>
    /// <param name="startTime">Time to start at</param>
    /// <param name="frameTime">Timestamp of the current video frame</param>
    /// <returns>Output bitmap</returns>
    unsafe Bitmap* GetVisualization(
        int width,
        int height,
        float pixelsPerMs,
        float amplitudeScale,
        long startTime,
        long frameTime
    );

    /// <summary>
    /// Release a frame
    /// </summary>
    /// <param name="frame">Pointer to frame to release</param>
    /// <returns>0 on success</returns>
    unsafe int ReleaseFrame(FrameGroup* frame);

    /// <summary>
    /// List of keyframes
    /// </summary>
    /// <returns></returns>
    int[] GetKeyframes();

    /// <summary>
    /// Timecode of each frame
    /// </summary>
    /// <returns></returns>
    long[] GetTimecodes();

    /// <summary>
    /// How long each frame is displayed for
    /// </summary>
    /// <returns></returns>
    long[] GetFrameIntervals();

    /// <summary>
    /// How many channels are in the audio
    /// </summary>
    /// <returns></returns>
    int GetChannelCount();

    /// <summary>
    /// Sample rate of the audio
    /// </summary>
    /// <returns></returns>
    int GetSampleRate();

    /// <summary>
    /// How many samples are in the audio
    /// </summary>
    /// <returns></returns>
    long GetSampleCount();

    /// <summary>
    /// Indexing progress
    /// </summary>
    /// <param name="current">Current progress</param>
    /// <param name="total">Goal progress</param>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    delegate void IndexingProgressCallback(long current, long total);
}

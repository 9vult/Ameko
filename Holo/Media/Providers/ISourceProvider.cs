// SPDX-License-Identifier: MPL-2.0

namespace Holo.Media.Providers;

public interface ISourceProvider
{
    /// <summary>
    /// Whether this provider is initialized
    /// </summary>
    bool IsInitialized { get; }

    /// <summary>
    /// Whether the loaded video has an audio track
    /// </summary>
    bool HasAudio { get; }

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
    void Initialize();

    /// <summary>
    /// Load a video
    /// </summary>
    /// <param name="filename">Path to the video to load</param>
    /// <returns>0 on success</returns>
    int LoadVideo(string filename);

    /// <summary>
    /// Load an audio file
    /// </summary>
    /// <param name="filename">Path to the audio to load</param>
    /// <returns>0 on success</returns>
    int LoadAudio(string filename, int audioTrackNumber = -1);

    /// <summary>
    /// Close the open video
    /// </summary>
    /// <returns>0 on success</returns>
    int CloseVideo();

    /// <summary>
    /// Close the open audio
    /// </summary>
    /// <returns>0 on success</returns>
    int CloseAudio();

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
    /// <returns>Audio frame</returns>
    unsafe AudioFrame* GetAudio();

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
    /// Get audio tracks informations
    /// </summary>
    /// <param name="filepath">Path to the video</param>
    /// <returns></returns>
    AudioTrack[] GetAudioTracks(string filepath);
}

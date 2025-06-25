// SPDX-License-Identifier: MPL-2.0

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
    void Initialize();

    /// <summary>
    /// Load a video
    /// </summary>
    /// <param name="filename">Path to the video to load</param>
    /// <returns>0 on success</returns>
    int LoadVideo(string filename);

    /// <summary>
    /// Close the open video
    /// </summary>
    /// <returns>0 on success</returns>
    int CloseVideo();

    /// <summary>
    /// Allocate frame buffers
    /// </summary>
    /// <param name="numBuffers"></param>
    /// <returns>0 on success</returns>
    int AllocateBuffers(int numBuffers);

    /// <summary>
    /// Free frame buffers
    /// </summary>
    /// <returns>0 on success</returns>
    int FreeBuffers();

    /// <summary>
    /// Get a frame
    /// </summary>
    /// <param name="frameNumber">Frame number to get</param>
    /// <param name="frame">Output frame</param>
    /// <returns>0 on success</returns>
    int GetFrame(int frameNumber, out VideoFrame frame);

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
}

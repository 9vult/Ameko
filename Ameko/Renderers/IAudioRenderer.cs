// SPDX-License-Identifier: GPL-3.0-only

namespace Ameko.Renderers;

public interface IAudioRenderer
{
    /// <summary>
    /// Initialize the renderer
    /// </summary>
    void Initialize();

    /// <summary>
    /// Play a portion of audio
    /// </summary>
    /// <param name="start">Start time in milliseconds</param>
    /// <param name="end">End time in milliseconds</param>
    void Play(long start, long end);

    /// <summary>
    /// Stop playing audio
    /// </summary>
    void Stop();
}

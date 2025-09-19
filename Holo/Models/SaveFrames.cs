// SPDX-License-Identifier: MPL-2.0

namespace Holo.Models;

/// <summary>
/// Where should frames be saved?
/// </summary>
public enum SaveFrames
{
    /// <summary>
    /// Save frames next to the video, with a filename based on the video
    /// </summary>
    WithVideo,

    /// <summary>
    /// Save frames next to the subtitles, with a filename based on the subs
    /// </summary>
    WithSubtitles,
}

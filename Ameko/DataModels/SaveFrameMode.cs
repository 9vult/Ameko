// SPDX-License-Identifier: GPL-3.0-only

namespace Ameko.DataModels;

/// <summary>
/// Method to save (or copy) a frame
/// </summary>
public enum SaveFrameMode
{
    /// <summary>
    /// Both the video and the subtitles will be saved
    /// </summary>
    Full,

    /// <summary>
    /// Just the video will be saved
    /// </summary>
    VideoOnly,

    /// <summary>
    /// Just the subtitles will be saved
    /// </summary>
    SubtitlesOnly,
}

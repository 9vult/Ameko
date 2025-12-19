// SPDX-License-Identifier: MPL-2.0

using System.ComponentModel;
using Holo.Media;
using Holo.Models;

namespace Holo.Configuration;

public interface IPersistence
{
    /// <summary>
    /// Name of the current <see cref="Layout"/>
    /// </summary>
    string LayoutName { get; set; }

    /// <summary>
    /// Whether the color picker should use a ring instead of a square
    /// </summary>
    bool UseColorRing { get; set; }

    /// <summary>
    /// Visualization scale in the X direction
    /// </summary>
    double VisualizationScaleX { get; set; }

    /// <summary>
    /// Visualization scale in the Y direction
    /// </summary>
    double VisualizationScaleY { get; set; }

    /// <summary>
    /// C# Playground
    /// </summary>
    string PlaygroundCs { get; set; }

    /// <summary>
    /// JavaScript Playground
    /// </summary>
    string PlaygroundJs { get; set; }

    /// <summary>
    /// Set the scale for a video resolution
    /// </summary>
    /// <param name="height">Height of the video in pixels</param>
    /// <param name="scaleFactor">Scale factor to use</param>
    void SetScaleForRes(int height, ScaleFactor scaleFactor);

    /// <summary>
    /// Get the scale factor for a video resolution
    /// </summary>
    /// <param name="height">Height of the video in pixels</param>
    /// <returns>The scale factor (or default)</returns>
    ScaleFactor GetScaleForRes(int height);

    /// <summary>
    /// Set the audio track to use for a video
    /// </summary>
    /// <param name="pathHash">Hashed video path</param>
    /// <param name="trackNumber">Track number</param>
    void SetAudioTrackForVideo(string pathHash, int trackNumber);

    /// <summary>
    /// Get the scale factor for a video resolution
    /// </summary>
    /// <param name="pathHash">Hashed video path</param>
    /// <returns>The scale factor (or -1)</returns>
    int GetAudioTrackForVideo(string pathHash);

    /// <summary>
    /// Write the persistence data to file
    /// </summary>
    /// <returns></returns>
    bool Save();

    event PropertyChangedEventHandler? PropertyChanged;
}

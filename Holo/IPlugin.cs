using AssCS;
using Holo.Utilities;
using System.Collections.Generic;

namespace Holo
{
    public interface IPlugin
    {
        /// <summary>
        /// The name of the plugin
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// Short description of the plugin
        /// </summary>
        public string Description { get; }
        /// <summary>
        /// Plugin version
        /// </summary>
        public double Version { get; }
        /// <summary>
        /// Author(s) of the plugin
        /// </summary>
        public string Author { get; }
        /// <summary>
        /// Contact information for the plugin author(s)
        /// </summary>
        public string AuthorUrl { get; }
        /// <summary>
        /// If the plugin is currently initialized
        /// </summary>
        public bool IsInitialized { get; }
        /// <summary>
        /// Set up the plugin
        /// </summary>
        /// <remarks>This should set <see cref="IsInitialized"/> to true</remarks>
        /// <returns>True if the plugin was successfully initialized</returns>
        public bool Initialize();
        /// <summary>
        /// Shut down the plugin
        /// </summary>
        /// <remarks>This should set <see cref="IsInitialized"/> to false</remarks>
        /// <returns>True if the plugin was successfully deinitialized</returns>
        public bool Deinitialize();
        /// <summary>
        /// Get the version of the library backing this plugin.
        /// </summary>
        /// <remarks>For example, a VapourSynth plugin should report the VapourSynth version</remarks>
        /// <returns>The version of the backing library</returns>
        public double GetBackingVersion();
        /// <summary>
        /// Properties
        /// </summary>
        /// <returns>Properties</returns>
        public IDictionary<string, dynamic> GetProperties();
    }

    public interface IVideoSourcePlugin : IPlugin
    {
        /// <summary>
        /// Open a video file
        /// </summary>
        /// <param name="filepath">Path to the video</param>
        /// <returns>True if the video was successfully opened</returns>
        public bool OpenFile(string filepath);
        public bool CloseFile();
        /// <summary>
        /// Get list of the tracks in the video
        /// </summary>
        /// <returns>List of track indexes</returns>
        public int[] GetVideoTracks();
        /// <summary>
        /// Load a particular track
        /// </summary>
        /// <param name="trackIdx">Track index</param>
        /// <returns>True if the track was successfully loaded</returns>
        public bool LoadTrack(int trackIdx);
        /// <summary>
        /// Get the number of frames in the video
        /// </summary>
        /// <returns></returns>
        public int GetFrameCount();
        /// <summary>
        /// Get the framerate of the video
        /// </summary>
        /// <remarks>
        /// This is only truly accurate in a constant-framerate (CFR) video,
        /// and should not be relied on. <see cref="GetFrameIntervals(long[])"/>
        /// should be used for accurate frame timing.
        /// </remarks>
        /// <returns></returns>
        public Rational GetFrameRate();
        /// <summary>
        /// Get a frame
        /// </summary>
        /// <param name="frameNumber">Frame number to get</param>
        /// <param name="frame">Frame object to put the data in</param>
        public void GetFrame(int frameNumber, ref VideoFrame frame);
        /// <summary>
        /// Get a list of timestamps for each frame
        /// </summary>
        /// <returns>List of millisecond timestamps</returns>
        public long[] GetFrameTimes();
        /// <summary>
        /// Get a list of how long a frame is to be displayed for
        /// </summary>
        /// <param name="frametimes">List of timestamps in milliseconds</param>
        /// <returns>List of millisecond lengths</returns>
        /// <seealso cref="GetFrameTimes"/>
        public float[] GetFrameIntervals(long[] frametimes);
        /// <summary>
        /// Get a list of keyframes
        /// </summary>
        /// <returns></returns>
        public int[] GetKeyframes();
    }

    public interface IAudioSourcePlugin : IPlugin
    {
        public bool OpenFile(string filepath);
        public bool CloseFile();
        public AudioFrame GetFrame(long frameNumber, int stream);
    }

    public interface ISubtitlePlugin : IPlugin
    {
        public void LoadSubtitles(string data);
        public void LoadSubtitles(File subs, int time = -1);
        public void DrawSubtitles(ref VideoFrame frame, long time);
        public void Reinitialize() { }
    }
}

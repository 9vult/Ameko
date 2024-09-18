using AssCS;
using Holo.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

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
        public bool OpenFile(string filepath);
        public bool CloseFile();
        public int[] GetVideoTracks();
        public bool LoadTrack(int track);
        public int GetFrameCount();
        public Rational GetFrameRate();
        public VideoFrame GetFrame(int frame);
        public long[] GetFrameTimes();
        public float[] GetFrameIntervals(long[] frametimes);
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
        public void DrawSubtitles(ref VideoFrame destination, long time);
        public void Reinitialize() { }
    }
}

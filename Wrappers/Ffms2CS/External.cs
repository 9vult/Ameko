using Ffms2CS.Structures;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Runtime.InteropServices;
using System.Text;
using static Ffms2CS.Ffms2;

namespace Ffms2CS
{
    /// <summary>
    /// External methods for FFMS2
    /// </summary>
    internal unsafe static class External
    {
        /// <summary>
        /// Initialize FFMS2
        /// </summary>
        /// <param name="zero1">Unused, pass 0</param>
        /// <param name="zero2">Unused, pass 0</param>
        [DllImport("ffms2", EntryPoint = "FFMS_Init")]
        public static extern void Init(int zero1, int zero2);

        /// <summary>
        /// Deinitialize FFMS2
        /// </summary>
        [DllImport("ffms2", EntryPoint = "FFMS_Deinit")]
        public static extern void Uninit();

        /// <summary>
        /// Get the FFMS2 library version
        /// </summary>
        /// <returns>Version integer</returns>
        [DllImport("ffms2", EntryPoint = "FFMS_GetVersion")]
        public static extern int GetVersion();

        /// <summary>
        /// Get the FFmpeg log message level
        /// </summary>
        /// <returns>FFmpeg log level</returns>
        [DllImport("ffms2", EntryPoint = "FFMS_GetLogLevel")]
        public static extern int GetLogLevel();

        /// <summary>
        /// Set the FFmpeg log message level
        /// </summary>
        /// <param name="level">Level to set</param>
        [DllImport("ffms2", EntryPoint = "FFMS_SetLogLevel")]
        public static extern void SetLogLevel(int level);

        /// <summary>
        /// Create a video source object
        /// </summary>
        /// <param name="sourceFile">Filepath to open</param>
        /// <param name="track">Track number to open</param>
        /// <param name="index">Indexing information for the track</param>
        /// <param name="threads">Number of decoding threads to use</param>
        /// <param name="seekMode">Controls how seeking is handled, see <see cref="SeekMode"/></param>
        /// <param name="errorInfo">ErrorInfo</param>
        /// <returns>Pointer to the created object on success, Null on failure</returns>
        [DllImport("ffms2", EntryPoint = "FFMS_CreateVideoSource")]
        public static extern IntPtr CreateVideoSource(byte[] sourceFile, int track, IndexPtr index, int threads, int seekMode, ref ErrorInfo errorInfo);

        /// <summary>
        /// Create an audio source object
        /// </summary>
        /// <param name="sourceFile">Filepath to open</param>
        /// <param name="track">Track number to open</param>
        /// <param name="index">Indexing information for the track</param>
        /// <param name="delayMode">Controls how audio with a nonzero first PTS is handled, see <see cref="AudioDelayMode"/></param>
        /// <param name="errorInfo">ErrorInfo</param>
        /// <returns>Pointer to the created object on success, Null on failure</returns>
        [DllImport("ffms2", EntryPoint = "FFMS_CreateAudioSource")]
        public static extern IntPtr CreateAudioSource(byte[] sourceFile, int track, IndexPtr index, int delayMode, ref ErrorInfo errorInfo);

        /// <summary>
        /// Create an audio source object, but with more options
        /// </summary>
        /// <param name="sourceFile">Filepath to open</param>
        /// <param name="track">Track number to open</param>
        /// <param name="index">Indexing information for the track</param>
        /// <param name="delayMode">Controls how audio with a nonzero first PTS is handled</param>
        /// <param name="fillGaps"></param>
        /// <param name="drcScale"></param>
        /// <param name="errorInfo">ErrorInfo</param>
        /// <returns>Pointer to the created object on success, Null on failure</returns>
        [DllImport("ffms2", EntryPoint = "FFMS_CreateAudioSource2")]
        public static extern IntPtr CreateAudioSource2(byte[] sourceFile, int track, IndexPtr index, int delayMode, int fillGaps, double drcScale, ref ErrorInfo errorInfo);

        /// <summary>
        /// Deallocate a video object
        /// </summary>
        /// <param name="videoSource">Pointer to the video source</param>
        [DllImport("ffms2", EntryPoint = "FFMS_DestroyVideoSource")]
        public static extern void DestroyVideoSource(IntPtr videoSource);

        /// <summary>
        /// Deallocate an audio object
        /// </summary>
        /// <param name="audioSource">Pointer to the audio source</param>
        [DllImport("ffms2", EntryPoint = "FFMS_DestroyAudioSource")]
        public static extern void DestroyAudioSource(IntPtr audioSource);

        /// <summary>
        /// Retrieve video properties
        /// </summary>
        /// <param name="videoSource">Pointer to the video source</param>
        /// <returns>Pointer to a VideoProperties struct</returns>
        [DllImport("ffms2", EntryPoint = "FFMS_GetVideoProperties")]
        public static extern IntPtr GetVideoProperties(IntPtr videoSource);

        /// <summary>
        /// Retrieve audio properties
        /// </summary>
        /// <param name="audioSource">Pointer to the audio source</param>
        /// <returns>Pointer to an AudioProperties struct</returns>
        [DllImport("ffms2", EntryPoint = "FFMS_GetAudioProperties")]
        public static extern IntPtr GetAudioProperties(IntPtr audioSource);

        /// <summary>
        /// Retrieve a given video frame
        /// </summary>
        /// <param name="videoSource">Pointer to the video source</param>
        /// <param name="num">Frame number to get</param>
        /// <param name="errorInfo">ErrorInfo</param>
        /// <returns>Pointer to the frame on success, Null on failure</returns>
        [DllImport("ffms2", EntryPoint = "FFMS_GetFrame")]
        public static extern NativeFrame* GetFrame(IntPtr videoSource, int num, ref ErrorInfo errorInfo);

        /// <summary>
        /// Retrieve a given video frame
        /// </summary>
        /// <param name="videoSource">Pointer to the video source</param>
        /// <param name="time">Timestamp in seconds</param>
        /// <param name="errorInfo">ErrorInfo</param>
        /// <returns>Pointer to the frame on success, Null on failure</returns>
        [DllImport("ffms2", EntryPoint = "FFMS_GetFrameByTime")]
        public static extern NativeFrame* GetFrameByTime(IntPtr videoSource, double time, ref ErrorInfo errorInfo);

        /// <summary>
        /// Decode a number of audio samples
        /// </summary>
        /// <param name="audioSource">Pointer to the audio source</param>
        /// <param name="buf">Buffer for the decoded samples</param>
        /// <param name="start">Start sample (inclusive)</param>
        /// <param name="count">Number of samples to decode (inclusive)</param>
        /// <param name="errorInfo">ErrorInfo</param>
        /// <returns>0 on success</returns>
        [DllImport("ffms2", EntryPoint = "FFMS_GetAudio")]
        public static extern int GetAudio(IntPtr audioSource, byte[] buf, long start, long count, ref ErrorInfo errorInfo);

        /// <summary>
        /// Set the output format for video frames
        /// </summary>
        /// <param name="videoSource">Pointer to the video source</param>
        /// <param name="targetFormats">-1 terminated list of acceptable colorspaces</param>
        /// <param name="width">Desired image width in pixels</param>
        /// <param name="height">Desired image height in pixels</param>
        /// <param name="resizer">Desired image resizing algorithm, see <see cref="Resizer"/></param>
        /// <param name="errorInfo">ErrorInfo</param>
        /// <returns>0 on success</returns>
        [DllImport("ffms2", EntryPoint = "FFMS_SetOutputFormatV2")]
        public static extern int SetOutputFormatV2(IntPtr videoSource, int[] targetFormats, int width, int height, int resizer, ref ErrorInfo errorInfo);

        /// <summary>
        /// Resets the video output format
        /// </summary>
        /// <param name="videoSource">Pointer to the video source</param>
        [DllImport("ffms2", EntryPoint = "FFMS_ResetOutputFormatV")]
        public static extern void ResetOutputFormatV(IntPtr videoSource);

        /// <summary>
        /// Override the source format for video frames
        /// </summary>
        /// <param name="videoSource">Pointer to the video source</param>
        /// <param name="colorSpace">The desired input colorspace, or Unspecified to leave it unchanged</param>
        /// <param name="colorRange">The desired input <see cref="ColorRange"/>, or Unspecified to leave it unchanged</param>
        /// <param name="format">The desired pixel format</param>
        /// <param name="errorInfo">ErrorInfo</param>
        /// <returns></returns>
        [DllImport("ffms2", EntryPoint = "FFMS_SetInputFormatV")]
        public static extern int SetInputFormatV(IntPtr videoSource, int colorSpace, int colorRange, int format, ref ErrorInfo errorInfo);

        /// <summary>
        /// Reset the video input format
        /// </summary>
        /// <param name="videoSource">Pointer to the video source</param>
        [DllImport("ffms2", EntryPoint = "FFMS_ResetInputFormatV")]
        public static extern void ResetInputFormatV(IntPtr videoSource);

        /// <summary>
        /// Create resample options
        /// </summary>
        /// <param name="audioSource">Pointer to the audio source</param>
        /// <returns>Pointer to the options</returns>
        [DllImport("ffms2", EntryPoint = "FFMS_CreateResampleOptions")]
        public static extern IntPtr CreateResampleOptions(IntPtr audioSource); // TODO: Check return type

        /// <summary>
        /// Set the output format for audio
        /// </summary>
        /// <param name="audioSource">Pointer to the audio source</param>
        /// <param name="options">Options to set</param>
        /// <param name="errorInfo">ErrorInfo</param>
        /// <returns>0 on success</returns>
        [DllImport("ffms2", EntryPoint = "FFMS_SetOutputFormatA")]
        public static extern int SetOutputFormatA(IntPtr audioSource, IntPtr options, ref ErrorInfo errorInfo); // TODO: check input

        [DllImport("ffms2", EntryPoint = "FFMS_DestroyResampleOptions")]
        public static extern void DestroyResampleOptions(IntPtr options); // TODO: check input

        /// <summary>
        /// Deallocate an index
        /// </summary>
        /// <param name="index">Pointer to the index</param>
        [DllImport("ffms2", EntryPoint = "FFMS_DestroyIndex")]
        public static extern void DestroyIndex(IntPtr index);

        /// <summary>
        /// Get the track number of the first track of a given type
        /// </summary>
        /// <param name="index">Pointer to the index</param>
        /// <param name="trackType"><see cref="TrackType"/></param>
        /// <param name="errorInfo">ErrorInfo</param>
        /// <returns>Track number, or negative number on failure</returns>
        [DllImport("ffms2", EntryPoint = "FFMS_GetFirstTrackOfType")]
        public static extern int GetFirstTrackOfType(IndexPtr index, int trackType, ref ErrorInfo errorInfo);

        /// <summary>
        /// Get the track number of the first track of a given type
        /// </summary>
        /// <remarks>
        /// Does the same thing as <see cref="GetFirstTrackOfType(IndexPtr, int, ref ErrorInfo)"/>
        /// but ignores unindexed tracks
        /// </remarks>
        /// <param name="index">Pointer to the index</param>
        /// <param name="trackType"><see cref="TrackType"/></param>
        /// <param name="errorInfo">ErrorInfo</param>
        /// <returns>Track number, or negative number on failure</returns>
        [DllImport("ffms2", EntryPoint = "FFMS_GetFirstIndexedTrackOfType")]
        public static extern int GetFirstIndexedTrackOfType(IndexPtr index, int trackType, ref ErrorInfo errorInfo);

        /// <summary>
        /// Get the number of tracks in an index
        /// </summary>
        /// <param name="index">Pointer to the index</param>
        /// <returns>Number of tracks</returns>
        [DllImport("ffms2", EntryPoint = "FFMS_GetNumTracks")]
        public static extern int GetNumTracks(IndexPtr index);

        /// <summary>
        /// Get the number of tracks in an indexer
        /// </summary>
        /// <param name="indexer">Pointer to the indexer</param>
        /// <returns>Number of tracks</returns>
        [DllImport("ffms2", EntryPoint = "FFMS_GetNumTracksI")]
        public static extern int GetNumTracksI(IndexerPtr indexer);

        /// <summary>
        /// Get the type of a track
        /// </summary>
        /// <param name="track">Pointer to the track</param>
        /// <returns>Integer representing the <see cref="TrackType"/></returns>
        [DllImport("ffms2", EntryPoint = "FFMS_GetTrackType")]
        public static extern int GetTrackType(IntPtr track);

        /// <summary>
        /// Get the type of track in an indexer
        /// </summary>
        /// <param name="indexer">Pointer to the indexer</param>
        /// <param name="track">Track number</param>
        /// <returns>Integer representing the <see cref="TrackType"/></returns>
        [DllImport("ffms2", EntryPoint = "FFMS_GetTrackTypeI")]
        public static extern int GetTrackTypeI(IndexerPtr indexer, int track);

        /// <summary>
        /// Get which error handling mode was used when creating the index
        /// </summary>
        /// <param name="index">Index to analyze</param>
        /// <returns>Error handling mode</returns>
        [DllImport("ffms2", EntryPoint = "FFMS_GetErrorHandling")]
        public static extern int GetErrorHandling(IndexPtr index);

        /// <summary>
        /// Get the name of the codec used for a track
        /// </summary>
        /// <param name="indexer">Pointer to the indexer</param>
        /// <param name="track">Track number</param>
        /// <returns>Human-readable name of the codec</returns>
        [DllImport("ffms2", EntryPoint = "FFMS_GetCodecNameI")]
        public static extern IntPtr GetCodecNameI(IndexerPtr indexer, int track);

        /// <summary>
        /// Get the name of the container format used for an indexer
        /// </summary>
        /// <param name="indexer">Pointer to the indexer</param>
        /// <returns>Human-readable name of the container</returns>
        [DllImport("ffms2", EntryPoint = "FFMS_GetFormatNameI")]
        public static extern IntPtr GetFormatNameI(IndexerPtr indexer);

        /// <summary>
        /// Get the number of frames in a track
        /// </summary>
        /// <param name="track">Pointer to the track</param>
        /// <returns>Number of frames for video (useful), number of packets for audio (not useful), 0 if not indexed</returns>
        [DllImport("ffms2", EntryPoint = "FFMS_GetNumFrames")]
        public static extern int GetNumFrames(IntPtr track);

        /// <summary>
        /// Get information about a frame
        /// </summary>
        /// <param name="track">Pointer to the track</param>
        /// <param name="frame">Frame number</param>
        /// <returns>Pointer to the FrameInfo struct on success, Null on failure</returns>
        [DllImport("ffms2", EntryPoint = "FFMS_GetFrameInfo")]
        public static extern IntPtr GetFrameInfo(IntPtr track, int frame);

        /// <summary>
        /// Get track information from an index
        /// </summary>
        /// <param name="index">Pointer to the index</param>
        /// <param name="track">Track number</param>
        /// <returns>Pointer to the Track</returns>
        [DllImport("ffms2", EntryPoint = "FFMS_GetTrackFromIndex")]
        public static extern IntPtr GetTrackFromIndex(IndexPtr index, int track);

        /// <summary>
        /// Retrieve track information from a video source
        /// </summary>
        /// <param name="videoSource">Pointer to the video source</param>
        /// <returns>Pointer to the Track</returns>
        [DllImport("ffms2", EntryPoint = "FFMS_GetTrackFromVideo")]
        public static extern IntPtr GetTrackFromVideo(IntPtr videoSource);

        /// <summary>
        /// Retrieve track information from an audio source
        /// </summary>
        /// <param name="videoSource">Pointer to the audio source</param>
        /// <returns>Pointer to the Track</returns>
        [DllImport("ffms2", EntryPoint = "FFMS_GetTrackFromAudio")]
        public static extern IntPtr GetTrackFromAudio(IntPtr audioSource);

        /// <summary>
        /// Retrieve the time base for the track
        /// </summary>
        /// <param name="track">Pointer to the track</param>
        /// <returns>Pointer to TrackTimeBase struct</returns>
        [DllImport("ffms2", EntryPoint = "FFMS_GetTimeBase")]
        public static extern IntPtr GetTimeBase(IntPtr track);

        /// <summary>
        /// Write timecodes for the track to disk
        /// </summary>
        /// <param name="track">Pointer to the track</param>
        /// <param name="timecodeFile">Filename to write to</param>
        /// <param name="errorInfo">ErrorInfo</param>
        /// <returns>0 on success</returns>
        [DllImport("ffms2", EntryPoint = "FFMS_WriteTimecodes")]
        public static extern int WriteTimecodes(IntPtr track, byte[] timecodeFile, ref ErrorInfo errorInfo);

        /// <summary>
        /// Create an indexer object for a file
        /// </summary>
        /// <remarks>
        /// Shorthand for <c>CreateIndexerWithDemuxer(SourceFile, FFMS_SOURCE_DEFAULT, ErrorInfo)</c>
        /// </remarks>
        /// <param name="sourceFile">Source filepath</param>
        /// <param name="errorInfo">ErrorInfo</param>
        /// <returns>Pointer to the indexer on success, Null on failure</returns>
        [DllImport("ffms2", EntryPoint = "FFMS_CreateIndexer")]
        public static extern IndexerPtr CreateIndexer(byte[] sourceFile, ref ErrorInfo errorInfo);

        /// <summary>
        /// Create an indexer object for a file
        /// </summary>
        /// <param name="sourceFile">Source filepath</param>
        /// <param name="demuxerOptions">Options for the demuxer</param>
        /// <param name="numOptions">Number of options being set</param>
        /// <param name="errorInfo">ErrorInfo</param>
        /// <returns>Pointer to the indexer on success, Null on failure</returns>
        [DllImport("ffms2", EntryPoint = "FFMS_CreateIndexer2")]
        public static extern IndexerPtr CreateIndexer2(byte[] sourceFile, ref Structures.KeyValuePair[] demuxerOptions, int numOptions, ref ErrorInfo errorInfo); // TODO: Check KVP

        /// <summary>
        /// Enable or disable indexing of a track
        /// </summary>
        /// <param name="indexer">Pointer to the indexer</param>
        /// <param name="track">Track number</param>
        /// <param name="index">0 to disable, other to enable</param>
        /// <param name="zero">Ignored, pass 0</param>
        [DllImport("ffms2", EntryPoint = "FFMS_TrackIndexSettings")]
        public static extern void TrackIndexSettings(IndexerPtr indexer, int track, int index, int zero);

        /// <summary>
        /// Enable or disable indexing of a track type
        /// </summary>
        /// <param name="indexer">Pointer to the indexer</param>
        /// <param name="trackKype"><see cref="TrackType"/></param>
        /// <param name="index">0 to disable, other to enable</param>
        /// <param name="zero">Ignored, pass 0</param>
        [DllImport("ffms2", EntryPoint = "FFMS_TrackTypeIndexSettings")]
        public static extern void TrackTypeIndexSettings(IndexerPtr indexer, int trackKype, int index, int zero);

        /// <summary>
        /// Set the callback function for indexing progress updates
        /// </summary>
        /// <param name="indexer">Pointer to the indexer</param>
        /// <param name="callback">Callback function</param>
        /// <param name="icPrivate"></param>
        [DllImport("ffms2", EntryPoint = "FFMS_SetProgressCallback")]
        public static extern void SetProgressCallback(IndexerPtr indexer, TIndexCallback callback, IntPtr icPrivate);

        /// <summary>
        /// Index the file represented by an indexer object
        /// </summary>
        /// <param name="indexer">Pointer to the indexer</param>
        /// <param name="errorHandling"><see cref="IndexErrorHandling"/></param>
        /// <param name="errorInfo">ErrorInfo</param>
        /// <returns>Pointer to the created index on success, Null on failure</returns>
        [DllImport("ffms2", EntryPoint = "FFMS_DoIndexing2")]
        public static extern IndexPtr DoIndexing2(IndexerPtr indexer, int errorHandling, ref ErrorInfo errorInfo);

        /// <summary>
        /// Destroys an indexer
        /// </summary>
        /// <param name="indexer">Pointer to the indexer</param>
        [DllImport("ffms2", EntryPoint = "FFMS_CancelIndexing")]
        public static extern void CancelIndexing(IntPtr indexer);

        /// <summary>
        /// Reads an index file from disk
        /// </summary>
        /// <param name="indexFile">Filepath</param>
        /// <param name="errorInfo">ErrorInfo</param>
        /// <returns>Pointer to Index on success, Null on failure</returns>
        [DllImport("ffms2", EntryPoint = "FFMS_ReadIndex")]
        public static extern IndexPtr ReadIndex(byte[] indexFile, ref ErrorInfo errorInfo);

        /// <summary>
        /// Reads an index from a user-supplied buffer
        /// </summary>
        /// <param name="bufferPointer">Pointer to the buffer</param>
        /// <param name="size">Size of the buffer</param>
        /// <param name="index">Index</param>
        /// <param name="errorInfo">ErrorInfo</param>
        /// <returns>Pointer to Index on success, Null on failure</returns>
        [DllImport("ffms2", EntryPoint = "FFMS_ReadIndexFromBuffer")]
        public static extern IndexPtr ReadIndexFromBuffer(IntPtr bufferPointer, int size, IndexPtr index, ref ErrorInfo errorInfo); // TODO: check

        /// <summary>
        /// Check if a given index belongs to a given file
        /// </summary>
        /// <param name="index">Pointer to the index</param>
        /// <param name="sourceFile">Filename</param>
        /// <param name="errorInfo">ErrorInfo</param>
        /// <returns>0 if true</returns>
        [DllImport("ffms2", EntryPoint = "FFMS_IndexBelongsToFile")]
        public static extern int IndexBelongsToFile(IndexPtr index, byte[] sourceFile, ref ErrorInfo errorInfo);

        /// <summary>
        /// Write an index object to disk
        /// </summary>
        /// <param name="indexFile">Filepath</param>
        /// <param name="index">Pointer to the index</param>
        /// <param name="errorInfo">ErrorInfo</param>
        /// <returns>0 on success</returns>
        [DllImport("ffms2", EntryPoint = "FFMS_WriteIndex")]
        public static extern int WriteIndex(byte[] indexFile, IndexPtr index, ref ErrorInfo errorInfo);

        /// <summary>
        /// Write an index to memory
        /// </summary>
        /// <param name="bufferPointer">Buffer pointer</param>
        /// <param name="size">Size of the buffer</param>
        /// <param name="index">Pointer to the index</param>
        /// <param name="errorInfo">ErrorInfo</param>
        /// <returns>0 on success</returns>
        [DllImport("ffms2", EntryPoint = "FFMS_WriteIndexToBuffer")]
        public static extern int WriteIndexToBuffer(IntPtr bufferPointer, int size, IndexPtr index, ref ErrorInfo errorInfo); // TODO: check

        /// <summary>
        /// Frees a buffer allocated by <see cref="WriteIndexToBuffer(IntPtr, int, IndexPtr, ref ErrorInfo)"/>
        /// </summary>
        /// <param name="bufferPointer">Pointer to the buffer</param>
        [DllImport("ffms2", EntryPoint = "FFMS_FreeIndexBuffer")]
        public static extern void FreeIndexBuffer(IntPtr bufferPointer);

        /// <summary>
        /// Get a colorspace identifier from a colorspace name
        /// </summary>
        /// <param name="name">Name of the colorspace</param>
        /// <returns>Integer representing the Pixel Format</returns>
        [DllImport("ffms2", EntryPoint = "FFMS_GetPixFmt")]
        public static extern int GetPixFmt([MarshalAs(UnmanagedType.LPStr)] string name);

        /// <summary>
        /// Indexing progress callback
        /// </summary>
        /// <param name="current">Current progress</param>
        /// <param name="total">Definition of 100%</param>
        /// <param name="icPrivate">Pointer to a progress object</param>
        /// <returns>0 to continue indexing, nonzero to cancel indexing</returns>
        public delegate int TIndexCallback(long current, long total, IntPtr icPrivate);
    }
}

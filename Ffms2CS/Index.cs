using Ffms2CS.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Ffms2CS
{
    /// <summary>
    /// FFMpeg Index
    /// </summary>
    public class IndexPtr : SafeHandle
    {
        private IndexPtr() : base(IntPtr.Zero, true) { }

        /// <summary>
        /// Returns true if the handle is null (IntPtr.Zero)
        /// </summary>
        public override bool IsInvalid => handle == IntPtr.Zero;

        /// <summary>
        /// Release the handle
        /// </summary>
        /// <returns>True</returns>
        protected override bool ReleaseHandle()
        {
            External.DestroyIndex(handle);
            return true;
        }
    }

    public class Index : IDisposable
    {
        private bool disposed = false;
        private readonly IndexPtr _ptr;

        /// <summary>
        /// The error handling being used for this index
        /// </summary>
        public IndexErrorHandling IndexErrorHandling => (IndexErrorHandling)External.GetErrorHandling(_ptr);

        /// <summary>
        /// The number of tracks in this index
        /// </summary>
        public int TrackCount => External.GetNumTracks(_ptr);

        /// <summary>
        /// Get the track number of the first track of a type
        /// </summary>
        /// <seealso cref="External.GetFirstTrackOfType(IndexPtr, int, ref ErrorInfo)"/>
        /// <param name="trackType">Type of track to look for</param>
        /// <returns>The track number</returns>
        /// <exception cref="Exception">The track type does not exist in the index</exception>
        public int GetFirstTrackOfType(TrackType trackType)
        {
            var errorInfo = Ffms2.NewErrorInfo();

            var track = External.GetFirstTrackOfType(_ptr, trackType: (int)trackType, ref errorInfo);

            if (track >= 0) return track;
            throw new Exception($"Exception getting track: {(Error)errorInfo.ErrorType}: {(Error)errorInfo.SubType}");
        }

        /// <summary>
        /// Get the track number of the first indexed track of a type
        /// </summary>
        /// <seealso cref="External.GetFirstIndexedTrackOfType(IndexPtr, int, ref ErrorInfo)"/>
        /// <param name="trackType">Type of track to look for</param>
        /// <returns>The track number</returns>
        /// <exception cref="Exception">The track type does not exist in the index</exception>
        public int GetFirstIndexedTrackOfType(TrackType trackType)
        {
            var errorInfo = Ffms2.NewErrorInfo();

            var track = External.GetFirstIndexedTrackOfType(_ptr, (int)trackType, ref errorInfo);

            if (track >= 0) return track;
            throw new Exception($"Exception getting track: {(Error)errorInfo.ErrorType}: {(Error)errorInfo.SubType}");
        }

        /// <summary>
        /// Write the index to a file
        /// </summary>
        /// <seealso cref="External.WriteIndex(byte[], IndexPtr, ref ErrorInfo)"/>
        /// <param name="filepath">Index filepath</param>
        /// <exception cref="ArgumentNullException">The filepath is null or empty</exception>
        /// <exception cref="Exception">There was an error writing the index</exception>
        public void WriteIndex(string filepath)
        {
            if (string.IsNullOrEmpty(filepath)) throw new ArgumentNullException(nameof(filepath));

            var errorInfo = Ffms2.NewErrorInfo();
            var bytes = Encoding.UTF8.GetBytes(filepath);

            var result = External.WriteIndex(bytes, _ptr, ref errorInfo);
            if (result == 0) return;

            throw new Exception($"Exception writing index: {(Error)errorInfo.ErrorType}: {(Error)errorInfo.SubType}");
        }

        // TODO: Write to buffer

        /// <summary>
        /// Check if a file is for this index
        /// </summary>
        /// <seealso cref="External.IndexBelongsToFile(IndexPtr, byte[], ref ErrorInfo)"/>
        /// <param name="filepath">Filepath to check</param>
        /// <returns>True if the file belongs to this index</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public bool BelongsToFile(string filepath)
        {
            if (string.IsNullOrEmpty(filepath)) throw new ArgumentNullException(nameof(filepath));

            var errorInfo = Ffms2.NewErrorInfo();
            var bytes = Encoding.UTF8.GetBytes(filepath);

            return External.IndexBelongsToFile(_ptr, bytes, ref errorInfo) == 0;
        }

        /// <summary>
        /// Create a video source
        /// </summary>
        /// <seealso cref="External.CreateVideoSource(byte[], int, IndexPtr, int, int, ref ErrorInfo)"/>
        /// <param name="filepath">Filepath to read from</param>
        /// <param name="track">Track to open</param>
        /// <param name="threads">Number of threads to use</param>
        /// <param name="seekMode">Seek mode to use</param>
        /// <returns>VideoSource object</returns>
        /// <exception cref="ArgumentNullException">The filepath was null or empty</exception>
        /// <exception cref="Exception">An error occured while opening the source</exception>
        public VideoSource VideoSource(string filepath, int track, int threads = 1, SeekMode seekMode = SeekMode.Normal)
        {
            if (string.IsNullOrEmpty(filepath)) throw new ArgumentNullException(nameof(filepath));
            
            var errorInfo = Ffms2.NewErrorInfo();
            var bytes = Encoding.UTF8.GetBytes(filepath);

            var sourcePtr = External.CreateVideoSource(bytes, track, _ptr, threads, (int)seekMode, ref errorInfo);
            if (sourcePtr != IntPtr.Zero)
                return new VideoSource(sourcePtr);
            throw new Exception($"Exception opening video source: {(Error)errorInfo.ErrorType}: {(Error)errorInfo.SubType}");
        }

        /// <summary>
        /// Create an audio source
        /// </summary>
        /// <seealso cref="External.CreateAudioSource(byte[], int, IndexPtr, int, ref ErrorInfo)"/>
        /// <param name="filepath">Filepath to read from</param>
        /// <param name="track">Track to open</param>
        /// <param name="delayMode">Delay mode to use</param>
        /// <returns>AudioSource object</returns>
        /// <exception cref="ArgumentNullException">The filepath was null or empty</exception>
        /// <exception cref="Exception">An error occured while opening the source</exception>
        public AudioSource AudioSource(string filepath, int track, AudioDelayMode delayMode = AudioDelayMode.DelayFirstVideoTrack)
        {
            if (string.IsNullOrEmpty(filepath)) throw new ArgumentNullException(nameof(filepath));

            var errorInfo = Ffms2.NewErrorInfo();

            var bytes = Encoding.UTF8.GetBytes(filepath);
            var source = External.CreateAudioSource(bytes, track, _ptr, (int)delayMode, ref errorInfo);

            if (source != IntPtr.Zero)
                return new AudioSource(source);
            throw new Exception($"Exception opening audio source: {(Error)errorInfo.ErrorType}: {(Error)errorInfo.SubType}");
        }

        /// <summary>
        /// Create an audio source
        /// </summary>
        /// <seealso cref="External.CreateAudioSource2(byte[], int, IndexPtr, int, int, double, ref ErrorInfo)"/>
        /// <param name="filepath">Filepath to read from</param>
        /// <param name="track">Track to open</param>
        /// <param name="fillGaps">Whether or not to fill gaps</param>
        /// <param name="drcScale">DRC scale</param>
        /// <param name="delayMode">Delay mode to use</param>
        /// <returns>AudioSource object</returns>
        /// <exception cref="ArgumentNullException">The filepath was null or empty</exception>
        /// <exception cref="Exception">An error occured while opening the source</exception>
        public AudioSource AudioSource(string filepath, int track, bool fillGaps, double drcScale, AudioDelayMode delayMode = AudioDelayMode.DelayFirstVideoTrack)
        {
            if (string.IsNullOrEmpty(filepath)) throw new ArgumentNullException(nameof(filepath));

            var errorInfo = Ffms2.NewErrorInfo();

            var bytes = Encoding.UTF8.GetBytes(filepath);
            var source = External.CreateAudioSource2(bytes, track, _ptr, (int)delayMode, fillGaps ? 1 : 0, drcScale, ref errorInfo);

            if (source != IntPtr.Zero)
                return new AudioSource(source);
            throw new Exception($"Exception opening audio source: {(Error)errorInfo.ErrorType}: {(Error)errorInfo.SubType}");
        } 

        /// <summary>
        /// Get a track object
        /// </summary>
        /// <param name="track">Track number to get</param>
        /// <returns>The track object</returns>
        /// <exception cref="ArgumentOutOfRangeException">The track number is out of range</exception>
        public Track GetTrack(int track)
        {
            if (track < 0 || track > External.GetNumTracks(_ptr)) throw new ArgumentOutOfRangeException(nameof(track));

            return new Track(External.GetTrackFromIndex(_ptr, track));
        }

        /// <summary>
        /// Read an index from an index file
        /// </summary>
        /// <param name="indexFilename">Index file name</param>
        /// <exception cref="ArgumentNullException">The index filename is null or empty</exception>
        /// <exception cref="FileLoadException">An error occured wh</exception>
        public Index(string indexFilename)
        {
            if (string.IsNullOrEmpty(indexFilename)) throw new ArgumentNullException(nameof(indexFilename));

            var errorInfo = Ffms2.NewErrorInfo();

            var bytes = Encoding.UTF8.GetBytes(indexFilename);
            _ptr = External.ReadIndex(bytes, ref errorInfo);

            if (_ptr.IsInvalid) throw new FileLoadException(errorInfo.Buffer, indexFilename);
        }

        // TODO: Load index from buffer

        /// <summary>
        /// Instantiate an Index from an index pointer
        /// </summary>
        /// <param name="ptr">Index pointer</param>
        public Index(IndexPtr ptr)
        {
            _ptr = ptr;
        }

        ~Index()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed) return;
            disposed = true;

            if (!disposing) return;
            if (_ptr != null && !_ptr.IsInvalid)
                _ptr.Dispose();
        }
    }
}

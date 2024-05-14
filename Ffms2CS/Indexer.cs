using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Ffms2CS
{
    /// <summary>
    /// FFMpeg Indexer Pointer
    /// </summary>
    public class IndexerPtr : SafeHandle
    {
        private IndexerPtr() : base(IntPtr.Zero, true) { }

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
            External.CancelIndexing(handle);
            return true;
        }
    }

    /// <summary>
    /// File indexer
    /// </summary>
    public class Indexer : IDisposable
    {
        private bool disposed = false;
        private readonly IndexerPtr _ptr;
        private External.TIndexCallback _indexingCallback;

        /// <summary>
        /// If the indexer is currently indexing
        /// </summary>
        public bool IsIndexing { get; private set; }
        /// <summary>
        /// If the indexer should cancel indexing
        /// </summary>
        public bool ShouldCancelIndexing { get; set; }

        /// <summary>
        /// The number of tracks in this Indexer
        /// </summary>
        /// <seealso cref="External.GetNumTracksI(IndexerPtr)"/>
        public int TrackCount => External.GetNumTracksI(_ptr);

        /// <summary>
        /// The human-readable format name
        /// </summary>
        /// <seealso cref="External.GetFormatNameI(IndexerPtr)"/>
        public string FormatName => Marshal.PtrToStringAnsi(External.GetFormatNameI(_ptr));

        /// <summary>
        /// Subscribable event for listening to indexing progress
        /// </summary>
        public event EventHandler<IndexingProgressChangedEventArgs>? IndexingProgressChanged;

        /// <summary>
        /// Subscribable event for when the indexing completes
        /// </summary>
        public event EventHandler<EventArgs>? IndexingCompleted;

        /// <summary>
        /// Indexing progress callback implementation
        /// </summary>
        /// <param name="current">Current progress</param>
        /// <param name="total">Definition of 100%</param>
        /// <param name="icPrivate">Pointer to a progress object</param>
        /// <returns>0 to continue indexing, nonzero to cancel indexing</returns>
        private int IndexingCallback(long current, long total, IntPtr icPrivate)
        {
            lock (this)
            {
                if (IndexingProgressChanged != null) IndexingProgressChanged(this, new IndexingProgressChangedEventArgs(current, total));
                if (IndexingCompleted == null) return ShouldCancelIndexing ? 1 : 0;

                if (current == total) IndexingCompleted(this, new EventArgs());
            }
            return ShouldCancelIndexing ? 1 : 0;
        }

        /// <summary>
        /// Get the type of track
        /// </summary>
        /// <param name="track">Track number to check</param>
        /// <seealso cref="External.GetTrackTypeI(IndexerPtr, int)"/>
        /// <returns>The track type</returns>
        public TrackType GetTrackType(int track)
        {
            if (!InRange(track)) return TrackType.Unknown; // Will not be returned

            return (TrackType)External.GetTrackTypeI(_ptr, track);
        }

        /// <summary>
        /// Get the name of the codec used for a track
        /// </summary>
        /// <seealso cref="External.GetCodecNameI(IndexerPtr, int)"/>
        /// <param name="track">Track number to check</param>
        /// <returns>The codec name</returns>
        public string GetCodecName(int track)
        {
            if (!InRange(track)) return string.Empty; // Will not be returned

            return Marshal.PtrToStringAnsi(External.GetCodecNameI(_ptr, track));
        }


        /// <summary>
        /// Set whether a track should be indexed or not
        /// </summary>
        /// <seealso cref="External.TrackIndexSettings(IndexerPtr, int, int, int)"/>
        /// <param name="track">Track number to set</param>
        /// <param name="index">If the track should be indexed</param>
        public void SetTrackShouldIndex(int track, bool index)
        {
            if (!InRange(track)) return;

            External.TrackIndexSettings(_ptr, track, index ? 1 : 0, 0);
        }

        /// <summary>
        /// Set whether a type of track should be indexed or not
        /// </summary>
        /// <seealso cref="External.TrackTypeIndexSettings(IndexerPtr, int, int, int)"/>
        /// <param name="trackType">Track type to set</param>
        /// <param name="index">If the track type should be indexed</param>
        /// <exception cref="ObjectDisposedException">The indexer was already indexed</exception>
        public void SetTrackTypeShouldIndex(TrackType trackType, bool index)
        {
            if (_ptr.IsInvalid) throw new ObjectDisposedException(nameof(_ptr));

            External.TrackTypeIndexSettings(_ptr, (int)trackType, index ? 1 : 0, 0);
        }

        /// <summary>
        /// Index the indexer!
        /// </summary>
        /// <seealso cref="External.DoIndexing2(IndexerPtr, int, ref ErrorInfo)"/>
        /// <param name="errorHandling">Error handling behavior</param>
        /// <returns>The index</returns>
        /// <exception cref="ObjectDisposedException">The indexer was already indexed</exception>
        /// <exception cref="Exception">Indexing failed</exception>
        public Index Index(IndexErrorHandling errorHandling = IndexErrorHandling.Abort)
        {
            if (_ptr.IsInvalid) throw new ObjectDisposedException(nameof(_ptr));

            var errorInfo = Ffms2.NewErrorInfo();

            IndexPtr indexPtr;
            IsIndexing = true;
            ShouldCancelIndexing = false;

            lock (this)
            {
                indexPtr = External.DoIndexing2(_ptr, (int)errorHandling, ref errorInfo);
            }

            _ptr.SetHandleAsInvalid();
            IsIndexing = false;

            if (indexPtr.IsInvalid)
            {
                throw new Exception($"Indexing exception: {(Error)errorInfo.ErrorType}: {(Error)errorInfo.SubType}");
            }

            return new Index(indexPtr);
        }

        /// <summary>
        /// Checks if the track is in range
        /// </summary>
        /// <param name="track">Track number</param>
        /// <returns>True if the track number is within range</returns>
        /// <exception cref="ObjectDisposedException">The indexer was already indexed</exception>
        /// <exception cref="ArgumentOutOfRangeException">The track number is out of range</exception>
        private bool InRange(int track)
        {
            if (_ptr.IsInvalid) throw new ObjectDisposedException(nameof(_ptr));
            if (track < 0 || track > TrackCount) throw new ArgumentOutOfRangeException(nameof(track));
            return true;
        }

        /// <summary>
        /// Create an indexer for a file
        /// </summary>
        /// <seealso cref="External.CreateIndexer(byte[], ref ErrorInfo)"/>
        /// <param name="sourceFilename">Filepath to the source file</param>
        /// <exception cref="ArgumentNullException">The filepath is null or empty</exception>
        /// <exception cref="FileLoadException">An error occured while loading the file</exception>
        public Indexer(string sourceFilename)
        {
            if (string.IsNullOrEmpty(sourceFilename)) throw new ArgumentNullException(nameof(sourceFilename));

            var errorInfo = Ffms2.NewErrorInfo();

            var bytes = Encoding.UTF8.GetBytes(sourceFilename);
            _ptr = External.CreateIndexer(bytes, ref errorInfo);

            if (_ptr.IsInvalid) throw new FileLoadException(errorInfo.Buffer, sourceFilename);

            _indexingCallback = new External.TIndexCallback(IndexingCallback);
            External.SetProgressCallback(_ptr, _indexingCallback, IntPtr.Zero);
        }

        ~Indexer()
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

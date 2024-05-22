using Ffms2CS.Enums;
using Ffms2CS.Structures;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Ffms2CS
{
    /// <summary>
    /// A track in the container
    /// </summary>
    public class Track
    {
        private readonly IntPtr _ptr;
        private readonly TrackTimeBase _timeBase;

        /// <summary>
        /// Time base unit
        /// </summary>
        public double Timebase => _timeBase.Num / (double)_timeBase.Den;

        /// <summary>
        /// Time base numerator
        /// </summary>
        public long TimebaseNumerator => _timeBase.Num;

        /// <summary>
        /// Time base denomerator
        /// </summary>
        public long TimebaseDenomerator => _timeBase.Den;

        /// <summary>
        /// Type of track
        /// </summary>
        public TrackType Type => (TrackType)External.GetTrackType(_ptr);

        /// <summary>
        /// Number of frames in the track
        /// </summary>
        public int FrameCount => External.GetNumFrames(_ptr);

        /// <summary>
        /// Write timecodes to a file
        /// </summary>
        /// <seealso cref="External.WriteTimecodes(IntPtr, byte[], ref ErrorInfo)"/>
        /// <param name="filepath">File to write to</param>
        /// <exception cref="ArgumentNullException">Filepath was null or empty</exception>
        /// <exception cref="Exception">Error occured while writing</exception>
        public void WriteTimecodes(string filepath)
        {
            if (string.IsNullOrEmpty(filepath)) throw new ArgumentNullException(nameof(filepath));
            var errorInfo = Ffms2.NewErrorInfo();

            var bytes = Encoding.UTF8.GetBytes(filepath);
            var result = External.WriteTimecodes(_ptr, bytes, ref errorInfo);

            if (result == 0) return;
            throw new Exception($"Exception writing timecodes: {(Error)errorInfo.ErrorType}: {(Error)errorInfo.SubType}");
        }

        /// <summary>
        /// Get information about a frame
        /// </summary>
        /// <seealso cref="External.GetFrameInfo(IntPtr, int)"/>
        /// <param name="frameNumber">Frame number to get</param>
        /// <returns>FrameInfo object</returns>
        /// <exception cref="InvalidOperationException">The track is not a video track</exception>
        public FrameInfo GetFrameInfo(int frameNumber)
        {
            if (Type != TrackType.Video) throw new InvalidOperationException("FrameInfo is only for video tracks!");
            return new FrameInfo((Structures.FrameInfo)Marshal.PtrToStructure(External.GetFrameInfo(_ptr, frameNumber), typeof(Structures.FrameInfo)));
        }

        /// <summary>
        /// Construct the track
        /// </summary>
        /// <param name="trackPtr">Pointer to the track</param>
        internal Track(IntPtr trackPtr)
        {
            _ptr = trackPtr;
            _timeBase = (TrackTimeBase)Marshal.PtrToStructure(External.GetTimeBase(_ptr), typeof(TrackTimeBase));
        }
    }
}

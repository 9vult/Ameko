using Ffms2CS.Enums;
using Ffms2CS.Structures;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Ffms2CS
{
    /// <summary>
    /// A video source
    /// </summary>
    public unsafe class VideoSource
    {
        private readonly IntPtr _ptr;
        private readonly VideoProperties _properties;
        internal Frame? LastFrame;
        private Track? _track;

        /// <summary>
        /// Frames per second
        /// </summary>
        public double FPS => _properties.FPSNumerator / (double)_properties.FPSDenominator;

        /// <summary>
        /// Numerator for fps
        /// </summary>
        public int FPSNumerator => _properties.FPSNumerator;

        /// <summary>
        /// Denominator for fps
        /// </summary>
        public int FPSDenominator => _properties.FPSDenominator;

        /// <summary>
        /// RFF timebase
        /// </summary>
        public double RFF => _properties.RFFNumerator / (double)_properties.RFFNumerator;

        /// <summary>
        /// Numerator for RFF timebase
        /// </summary>
        public int RFFNumerator => _properties.RFFNumerator;

        /// <summary>
        /// Denominator for RFF timebase
        /// </summary>
        public int RFFDenominator => _properties.RFFDenominator;

        /// <summary>
        /// Screen aspect ratio
        /// </summary>
        public double SAR => _properties.SARNum / (double)_properties.SARDen;

        /// <summary>
        /// Numerator for scren aspect ratio
        /// </summary>
        public int SARNumerator => _properties.SARNum;

        /// <summary>
        /// Denominator for screen aspect ratio
        /// </summary>
        public int SARDenominator => _properties.SARDen;

        /// <summary>
        /// Number of frames in this source
        /// </summary>
        public int FrameCount => _properties.NumFrames;

        /// <summary>
        /// Rectangle representing how far to crop in each side of the frame
        /// </summary>
        public Crop Crop => new Crop(_properties.CropTop, _properties.CropLeft, _properties.CropRight, _properties.CropBottom);

        /// <summary>
        /// If interlaced, is the top field first?
        /// </summary>
        public bool IsTopFieldFirst => _properties.TopFieldFirst != 0;

        /// <summary>
        /// First timestamp
        /// </summary>
        public double FirstTime => _properties.FirstTime;

        /// <summary>
        /// Last timestamp
        /// </summary>
        public double LastTime => _properties.LastTime;

        /// <summary>
        /// Get the track for this video source
        /// </summary>
        public Track Track
        {
            get
            {
                if (_track != null) return _track;

                _track = new Track(External.GetTrackFromVideo(_ptr));
                return _track;
            }
        }

        /// <summary>
        /// Set the output format
        /// </summary>
        /// <seealso cref="External.SetOutputFormatV2(IntPtr, int[], int, int, int, ref ErrorInfo)"/>
        /// <param name="targetFormats">Desired formats for output</param>
        /// <param name="width">Output width</param>
        /// <param name="height">Output height</param>
        /// <param name="resizer">Resizing algorithm to use</param>
        /// <exception cref="ArgumentNullException">Target formats was null</exception>
        /// <exception cref="ArgumentOutOfRangeException">Width or height was out of range</exception>
        /// <exception cref="Exception">An error occured while setting the format</exception>
        public void SetOutputFormat(ICollection<int> targetFormats, int width, int height, Resizer resizer)
        {
            if (targetFormats == null) throw new ArgumentNullException(nameof(targetFormats));
            if (width < 0) throw new ArgumentOutOfRangeException(nameof(width));
            if (height < 0) throw new ArgumentOutOfRangeException(nameof(height));

            var errorInfo = Ffms2.NewErrorInfo();

            var targetFormatsArray = new int[targetFormats.Count + 1];
            targetFormats.CopyTo(targetFormatsArray, 0);
            targetFormatsArray[targetFormats.Count] = -1;

            InvalidateLastFrame();

            if (External.SetOutputFormatV2(_ptr, targetFormatsArray, width, height, (int)resizer, ref errorInfo) == 0)
                return;
            throw new Exception($"Exception setting output format: {(Error)errorInfo.ErrorType}: {(Error)errorInfo.SubType}");
        }

        /// <summary>
        /// Reset the video output format
        /// </summary>
        /// <seealso cref="External.ResetOutputFormatV(IntPtr)"/>
        public void ResetOutputFormat()
        {
            External.ResetOutputFormatV(_ptr);
            InvalidateLastFrame();
        }

        /// <summary>
        /// Set the video frame input format
        /// </summary>
        /// <seealso cref="External.SetInputFormatV(IntPtr, int, int, int, ref ErrorInfo)"/>
        /// <param name="format">Pixel format</param>
        /// <param name="colorSpace">Color space</param>
        /// <param name="colorRange">Coplor range</param>
        /// <exception cref="Exception">An error occured while setting the input format</exception>
        public void SetInputFormat(int format, int colorSpace = 0, ColorRange colorRange = ColorRange.Unspecified)
        {
            var errorInfo = Ffms2.NewErrorInfo();
            InvalidateLastFrame();

            if (External.SetInputFormatV(_ptr, colorSpace, (int)colorRange, format, ref errorInfo) == 0)
                return;
            throw new Exception($"Exception setting input format: {(Error)errorInfo.ErrorType}: {(Error)errorInfo.SubType}");
        }

        /// <summary>
        /// Set the video frame input format
        /// </summary>
        /// <seealso cref="SetInputFormat(int, int, ColorRange)"/>
        /// <param name="colorSpace">Color space</param>
        /// <param name="colorRange">Color range</param>
        public void SetInputFormat(int colorSpace = 0, ColorRange colorRange = ColorRange.Unspecified)
        {
            this.SetInputFormat(Ffms2.GetPixelFormat(""), colorSpace, colorRange);
        }

        /// <summary>
        /// Reset the input format
        /// </summary>
        /// <seealso cref="External.ResetInputFormatV(IntPtr)"/>
        public void ResetInputFormat()
        {
            External.ResetInputFormatV(_ptr);
            InvalidateLastFrame();
        }

        /// <summary>
        /// Get a frame
        /// </summary>
        /// <seealso cref="External.GetFrame(IntPtr, int, ref ErrorInfo)"/>
        /// <param name="frameNumber">Frame number to get</param>
        /// <returns>Frame object</returns>
        /// <exception cref="ArgumentOutOfRangeException">The frame number is out of bounds</exception>
        /// <exception cref="Exception">An error occured while getting the frame</exception>
        public Frame GetFrame(int frameNumber)
        {
            if (frameNumber < 0 || frameNumber > FrameCount - 1) throw new ArgumentOutOfRangeException(nameof(frameNumber));
            var errorInfo = Ffms2.NewErrorInfo();
            InvalidateLastFrame();

            NativeFrame* framePointer;
            lock (this)
            {
                framePointer = External.GetFrame(_ptr, frameNumber, ref errorInfo);
            }

            if (framePointer != null)
            {
                LastFrame = new Frame(framePointer);
                return LastFrame;
            }
            throw new Exception($"Exception getting frame: {(Error)errorInfo.ErrorType}: {(Error)errorInfo.SubType}");
        }

        /// <summary>
        /// Get a frame
        /// </summary>
        /// <seealso cref="External.GetFrameByTime(IntPtr, double, ref ErrorInfo)"/>
        /// <param name="time">Timestamp to get the frame at</param>
        /// <returns>Frame object</returns>
        /// <exception cref="ArgumentOutOfRangeException">Time is out of bounds</exception>
        /// <exception cref="Exception">An error occured while getting the frame</exception>
        public Frame GetFrameAtTime(double time)
        {
            if (time < 0 || time > LastTime) throw new ArgumentOutOfRangeException(nameof(time));
            var errorInfo = Ffms2.NewErrorInfo();
            InvalidateLastFrame();

            NativeFrame* framePointer;
            lock (this)
            {
                framePointer = External.GetFrameByTime(_ptr, time, ref errorInfo);
            }

            if (framePointer != null)
            {
                LastFrame = new Frame(framePointer);
                return LastFrame;
            }
            throw new Exception($"Exception getting frame: {(Error)errorInfo.ErrorType}: {(Error)errorInfo.SubType}");
        }

        /// <summary>
        /// Invalidate the last frame and set it to null
        /// </summary>
        private void InvalidateLastFrame()
        {
            if (LastFrame != null)
                LastFrame.Invalid = true;
            LastFrame = null;
        }

        internal VideoSource(IntPtr sourcePtr)
        {
            _ptr = sourcePtr;
            _properties = (VideoProperties)Marshal.PtrToStructure(External.GetVideoProperties(sourcePtr), typeof(VideoProperties));
        }

        /// <summary>
        /// Destroy the video source
        /// </summary>
        /// <seealso cref="External.DestroyVideoSource(IntPtr)"/>
        ~VideoSource()
        {
            InvalidateLastFrame();
            External.DestroyVideoSource(_ptr);
        }

    }
}

using AssCS;
using Holo.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

namespace Holo
{
    public class VideoWrapper : INotifyPropertyChanged
    {
        private int _currentFrame;
        private readonly int _frameCount;
        private readonly Rational _sar;
        private readonly Rational _frameRate;

        public int CurrentFrame
        {
            get => _currentFrame;
            set
            {
                _currentFrame = value;
                OnPropertyChanged(nameof(CurrentFrame));
                OnPropertyChanged(nameof(CurrentTimeEstimated));
            }
        }

        /// <summary>
        /// Current timestamp based on the current frame
        /// </summary>
        public Time CurrentTimeEstimated => FrameToTime(_currentFrame);

        // Primaries
        public int FrameCount => _frameCount;
        public Rational SAR => _sar;
        public Rational FrameRate => _frameRate;

        // Methods

        /// <summary>
        /// Get the frame closest to a millisecond value (Rounding Method)
        /// </summary>
        /// <remarks>Based on https://github.com/moi15moi/PyonFX/blob/FrameUtility-V3/docs/Proof%20algorithm%20-%20ms_to_frames.md</remarks>
        /// <param name="milliseconds">Milliseconds</param>
        /// <returns>Frame number</returns>
        public int MillisToFrame(long milliseconds)
        {
            return (int)Math.Round((milliseconds + 0.5) * _frameRate.Ratio * (1 / (double)1000));
        }

        /// <summary>
        /// Get the milliseconds for a frame
        /// </summary>
        /// <param name="frame">Frame number</param>
        /// <returns>Milliseconds for the frame</returns>
        public long FrameToMillis(int frame)
        {
            return (long)((frame / _frameRate.Ratio) * 1000);
        }

        /// <summary>
        /// Get the frame closest to a Time value (Rounding Method)
        /// </summary>
        /// <param name="time">Time to get the frame at</param>
        /// <returns>Frame number</returns>
        public int TimeToFrame(Time time)
        {
            return MillisToFrame(time.TotalMilliseconds);
        }

        /// <summary>
        /// Get a Time for a frame
        /// </summary>
        /// <param name="frame">Frame number</param>
        /// <returns>Time object set to the time of the frame</returns>
        public Time FrameToTime(int frame)
        {
            return Time.FromMillis(FrameToMillis(frame));
        }

        // Extras for GUI support
        public int __FrameCountZeroIndex => _frameCount - 1;
        public int __FrameRateCeiling => (int)Math.Ceiling(_frameRate.Ratio);

        public VideoWrapper(int frameCount, Rational sar, Rational frameRate)
        {
            _currentFrame = 0;
            _frameCount = frameCount;
            _sar = sar;
            _frameRate = frameRate;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
